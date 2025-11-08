using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Handlers.Payments.Commands.CreateCheckoutSessionForBooking;
using PaymentService.Application.Handlers.Payments.Queries.GetUserPayments;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("booking/{bookingId}/checkout-session")]
    public async Task<IActionResult> CreateCheckoutSessionForBooking(
        [FromRoute] Guid bookingId,
        [FromBody] CreateCheckoutSessionRequest request)
    {
        var command = new CreateCheckoutSessionForBookingCommand(
            bookingId,
            request.UserId,
            request.Amount,
            request.OrderName);
        var sessionUrl = await _mediator.Send(command);
        return Ok(new { sessionUrl });
    }

    public record CreateCheckoutSessionRequest(Guid UserId, decimal Amount, string OrderName);


    [HttpGet("user/{userId}")]
    [Authorize(Policy = "UserOrAdminPolicy")]
    public async Task<IActionResult> GetPaymentsByUserId([FromRoute] Guid userId)
    {
        var query = new GetUserPaymentsQuery(userId);
        var payments = await _mediator.Send(query);
        return Ok(payments);
    }
}