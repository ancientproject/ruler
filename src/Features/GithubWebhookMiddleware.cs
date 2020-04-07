namespace ruler.Features
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class GithubWebhookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GithubWebhookMiddleware> _logger;

        public GithubWebhookMiddleware(RequestDelegate next, ILogger<GithubWebhookMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.IsFromGithubHookspot())
            {
                await _next(context);
                return;
            }

            var delivery = context.GetGithubDeliveryID();
            var @event = context.GetGithubEvent();


            _logger.LogInformation($"New github hook with delivery id: {delivery} and event: {@event}");

            context.Request.Path = $"/api/@me/hook/{@event}";
            await _next(context);
        }
    }
}