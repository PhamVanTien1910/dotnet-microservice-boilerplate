using BuildingBlocks.MediatR.Abstractions.Command;

namespace PaymentService.Application.Handlers.Payments.Commands.ProcessStripeEvent;

public record ProcessStripeEventCommand(
    string StripeSessionId,
    Dictionary<string, string> Metadata) : ICommand;