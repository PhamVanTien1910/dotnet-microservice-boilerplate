using BuildingBlocks.Application.Messaging.Abstractions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;
using PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;
using Shared.IntegrationEvents;

namespace PaymentService.Application.Handlers.Payments.Commands.ProcessStripeEvent;

public class ProcessStripeEventCommandHandler : ICommandHandler<ProcessStripeEventCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProcessStripeEventCommandHandler> _logger;

    public ProcessStripeEventCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<ProcessStripeEventCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(ProcessStripeEventCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Stripe event for session {SessionId}", request.StripeSessionId);

        try
        {
            Domain.Aggregates.PaymentAggregate.Entities.Payment? payment = null;

            if (request.Metadata.TryGetValue("paymentId", out var paymentIdStr) &&
                Guid.TryParse(paymentIdStr, out var paymentId))
            {
                payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            }

            if (payment == null)
            {
                var spec = new PaymentByStripeSessionIdSpecification(request.StripeSessionId);
                payment = await _paymentRepository.GetBySpecAsync(spec, cancellationToken);
            }

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Stripe session {SessionId}", request.StripeSessionId);
                return;
            }

            payment.MarkAsSucceeded();
            _paymentRepository.Update(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment {PaymentId} marked as succeeded", payment.Id);

            if (payment.BookingId.HasValue)
            {
                var paymentSucceededEvent = new PaymentSucceededForBookingIntegrationEvent(
                    payment.BookingId!.Value,
                    payment.ShopId!.Value,
                    payment.Id,
                    payment.Amount
                );
                await _eventBus.PublishAsync(paymentSucceededEvent, cancellationToken);
                _logger.LogInformation("PaymentSucceededForBookingIntegrationEvent published for booking {BookingId}",
                    payment.BookingId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe event for session {SessionId}", request.StripeSessionId);
            throw;
        }
    }
}