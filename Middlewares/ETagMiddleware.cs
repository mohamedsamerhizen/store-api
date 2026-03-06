using System.Security.Cryptography;
using System.Text;

namespace store.Middlewares
{
    public class ETagMiddleware
    {
        private readonly RequestDelegate _next;

        public ETagMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBody = context.Response.Body;

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            if (context.Response.StatusCode == 200)
            {
                memoryStream.Position = 0;

                var body = await new StreamReader(memoryStream).ReadToEndAsync();

                var hash = Convert.ToBase64String(
                    SHA256.HashData(Encoding.UTF8.GetBytes(body)));

                context.Response.Headers["ETag"] = hash;

                if (context.Request.Headers.TryGetValue("If-None-Match", out var incomingETag))
                {
                    if (incomingETag == hash)
                    {
                        context.Response.StatusCode = StatusCodes.Status304NotModified;
                        return;
                    }
                }

                var bytes = Encoding.UTF8.GetBytes(body);
                context.Response.ContentLength = bytes.Length;

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBody);
            }
        }
    }
}