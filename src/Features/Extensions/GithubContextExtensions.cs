namespace ruler.Features
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    public static class GithubContextExtensions
    {
        public static bool IsFromGithubHookspot(this HttpContext http) 
            => http.Request.Headers["User-Agent"].Contains("GitHub-Hookshot/");
        public static string GetGithubEvent(this HttpContext http)
            => http.Request.Headers["X-GitHub-Event"];
        public static Guid GetGithubDeliveryID(this HttpContext http)
            => Guid.Parse(http.Request.Headers["X-GitHub-Delivery"].ToString() ?? Guid.Empty.ToString());
    }
}