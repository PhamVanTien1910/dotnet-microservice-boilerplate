using BuildingBlocks.MediatR.Abstractions.Command;

namespace PaymentService.Application.Handlers.Payments.Commands.CreateCheckoutSessionForBooking;

public record CreateCheckoutSessionForBookingCommand(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string ProductName) : ICommand<string>;