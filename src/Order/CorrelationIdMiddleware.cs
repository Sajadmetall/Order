namespace Order
{
    public sealed class CorrelationIdMiddleware : IMiddleware
    {
        private const string HeaderName = "X-Correlation-Id";
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

            context.Response.Headers[HeaderName] = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                await next(context);
            }
        }
    }
}
