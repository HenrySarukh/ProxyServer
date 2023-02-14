using System;
using System.Net;
using System.Text.RegularExpressions;

namespace VMedia_Task.Middleware
{
    public class HabrMiddleware2
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HabrMiddleware2> _logger;
        private readonly Regex _sixLetterWordsRegex = new Regex("<.*?>(.*?)</.*?>", RegexOptions.Compiled);

        public HabrMiddleware2(RequestDelegate next, ILogger<HabrMiddleware2> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                var habrUri = new Uri("https://habr.com" + request.Path);
                using var habrClient = new WebClient();
                var habrHtml = await habrClient.DownloadStringTaskAsync(habrUri);

                MatchCollection matches = _sixLetterWordsRegex.Matches(habrHtml);

                foreach (Match match in matches)
                {
                    string tagText = match.Groups[1].Value;
                    string modifiedTagText = Regex.Replace(tagText, @"\b\w{6}\b", word => word.Value + "™");
                    habrHtml = habrHtml.Replace(tagText, modifiedTagText);
                }

                // Replace all internal navigation links with the address of the proxy
                habrHtml = habrHtml.Replace("href=\"/", $"href=\"{request.Scheme}://{request.Host}:{request.Host.Port}/");

                //// Set the modified HTML as the response
                response.ContentType = "text/html";
                await response.WriteAsync(habrHtml);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while proxying request to Habr");
                response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }

}

