using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentService.Application.Handlers.Payments.Commands.ProcessStripeEvent;
using PaymentService.Infrastructure.Settings;
using Stripe;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly string _webhookSecret;

    public WebhooksController(IMediator mediator, IOptions<StripeSettings> stripeSettings)
    {
        _mediator = mediator;
        _webhookSecret = stripeSettings.Value.WebhookSecret;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = HttpContext.Items["StripeRawBody"] as string;

        if (string.IsNullOrEmpty(json))
        {
            json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        }

        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            Event stripeEvent;

            if (isDevelopment && string.IsNullOrEmpty(stripeSignature))
            {
                stripeEvent = EventUtility.ParseEvent(json);
                Console.WriteLine($"Development mode: Processing webhook without signature validation");
            }
            else
            {
                if (string.IsNullOrEmpty(stripeSignature))
                {
                    return BadRequest("Missing Stripe-Signature header");
                }

                if (string.IsNullOrEmpty(_webhookSecret))
                {
                    return BadRequest("Webhook secret not configured");
                }

                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session != null)
                {
                    var command = new ProcessStripeEventCommand(session.Id, session.Metadata);
                    await _mediator.Send(command);
                }
            }
            else
            {
                Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { Error = "Invalid Stripe signature.", Details = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = "Webhook processing failed.", Details = ex.Message });
        }
    }

    // Manual endpoint for development/testing when webhooks aren't available
    [HttpPost("manual-complete/{sessionId}")]
    public async Task<IActionResult> ManualCompletePayment(string sessionId)
    {
        try
        {
            
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            if (!isDevelopment)
            {
                return BadRequest("This endpoint is only available in development environment");
            }

            // Create a mock Stripe session object with the sessionId
            var mockSession = new Stripe.Checkout.Session
            {
                Id = sessionId,
                Metadata = new Dictionary<string, string>()
            };

            var command = new ProcessStripeEventCommand(sessionId, new Dictionary<string, string>());
            await _mediator.Send(command);

            return Ok(new { Message = "Payment completion processed manually", SessionId = sessionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = "Failed to process manual payment completion", Details = ex.Message });
        }
    }
}