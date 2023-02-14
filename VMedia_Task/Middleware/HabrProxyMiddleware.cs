﻿using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ProxyServer
{
    public class HabrProxyMiddleware
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _next;

        public HabrProxyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var targetUri = BuildTargetUri(context.Request);

                if (targetUri != null)
                {
                    var targetRequestMessage = CreateTargetMessage(context, targetUri);

                    using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        context.Response.StatusCode = (int)responseMessage.StatusCode;
                        CopyFromTargetResponseHeaders(context, responseMessage);
                        await ProcessResponseContent(context, responseMessage);
                        //await responseMessage.Content.CopyToAsync(context.Response.Body);
                    }
                    return;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await _next(context);
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }

        private HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private Uri BuildTargetUri(HttpRequest request)
        {
            Uri? targetUri = null;
            PathString remainingPath;

            if (request.Path.StartsWithSegments("/habrDotNet", out remainingPath))
            {
                targetUri = new Uri("https://habr.com" + remainingPath);
            }

            if (request.Path.StartsWithSegments("/assetsHabr", out remainingPath))
            {
                targetUri = new Uri("https://assets.habr.com" + remainingPath);
            }

            //if (request.Path.StartsWithSegments("/skcrtxr", out remainingPath))
            //{
            //    targetUri = new Uri("https://skcrtxr.com" + remainingPath);
            //}

            return targetUri ?? new Uri("https://habr.com/en/all/");
            //return targetUri ?? new Uri("https://habr.com" + request.Path);
        }

        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage)
        {
            context.Response.Headers.Remove("Content-Length");
            var content = await responseMessage.Content.ReadAsByteArrayAsync();

            if (IsContentOfType(responseMessage, "text/html") ||
                IsContentOfType(responseMessage, "text/javascript"))
                //IsContentOfType(responseMessage, "application/javascript"))
            {
                var stringContent = Encoding.UTF8.GetString(content);
                var newContent = stringContent
                    .Replace("https://habr.com", "/habrDotNet")
                    .Replace("https://assets.habr.com", "/assetsHabr");
                await context.Response.WriteAsync(newContent, Encoding.UTF8);
            }
            else
            {
                await context.Response.Body.WriteAsync(content);
            }
        }

        private bool IsContentOfType(HttpResponseMessage responseMessage, string type)
        {
            var result = false;

            if (responseMessage.Content?.Headers?.ContentType != null)
            {
                result = responseMessage.Content.Headers.ContentType.MediaType == type;
            }

            return result;
        }
    }
}