using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace ProxyServer
{
    [ApiController]
    [Route("aramiysi/ara")]
    public class ProxyController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(string? url)
        {
            HttpClient client = new HttpClient();
            var _params = !string.IsNullOrEmpty(url) ? Uri.EscapeDataString(url) : "";
            HttpResponseMessage response = await client.GetAsync($"https://habr.com{Request.Path}");
            string responseString = await response.Content.ReadAsStringAsync();

            return Content(responseString, "text/html");
        }
    }
}
