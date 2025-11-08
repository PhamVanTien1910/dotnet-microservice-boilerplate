using System.Text;

namespace PaymentService.API.Middleware;

public class StripeWebhookMiddleware
{
    private readonly RequestDelegate _next;

    public StripeWebhookMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsStripeWebhookRequest(context.Request))
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            context.Items["StripeRawBody"] = rawBody;
            
            context.Request.Body.Position = 0;
        }

        await _next(context);
    }

    private static bool IsStripeWebhookRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api") && 
               request.Path.Value?.Contains("/webhooks/stripe") == true;
    }
}
