using System.Net;
using System.Text.Json;
using store.Common;

namespace store.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Unhandled server error occurred.");
            }
            else
            {
                _logger.LogWarning(exception, "Handled application exception occurred.");
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = statusCode == (int)HttpStatusCode.InternalServerError
                ? ApiResponse.FailResponse("An unexpected error occurred.")
                : ApiResponse.FailResponse(exception.Message);

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}