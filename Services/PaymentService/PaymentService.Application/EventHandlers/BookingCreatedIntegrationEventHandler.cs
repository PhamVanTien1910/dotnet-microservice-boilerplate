using BuildingBlocks.Application.Messaging.Abstractions;
using BuildingBlocks.Domain.Repositories;
using Microsoft.Extensions.Options;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;
using PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;
using PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;
using PaymentService.Domain.Aggregates.PaymentAggregate.ValueObjects;
using PaymentService.Application.Settings;
using Shared.IntegrationEvents;

namespace PaymentService.Application.EventHandlers;

public class BookingCreatedIntegrationEventHandler : IIntegrationEventHandler<BookingCreatedIntegrationEvent>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly PaymentSettings _paymentSettings;

    public BookingCreatedIntegrationEventHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IPaymentGatewayService paymentGatewayService,
        IOptions<PaymentSettings> paymentSettings)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _paymentGatewayService = paymentGatewayService;
        _paymentSettings = paymentSettings.Value;
    }

    public async Task HandleAsync(BookingCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        var existingPayment =
            await _paymentRepository.GetBySpecAsync(new PaymentByBookingIdSpecification(@event.BookingId),
                cancellationToken);
        if (existingPayment is not null)
            return;

        var newPayment = Payment.Create(
        @event.UserId,
        @event.TotalAmount,
        _paymentSettings.PaymentMethodType,
        Currency.USD,
        @event.BookingId,
        @event.ShopId);

        await _paymentRepository.AddAsync(newPayment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var checkoutSession = await CreateCheckoutSessionAsync(newPayment, @event, cancellationToken);

        newPayment.AssociateWithStripeSession(checkoutSession.SessionId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var checkoutSessionCreatedEvent = new BookingCheckoutSessionCreatedIntegrationEvent(
            @event.BookingId,
            @event.UserId,
            @event.UserEmail,
            @event.ProductName,
            @event.TotalAmount,
            @event.Currency,
            @event.NumberOfLessons,
            checkoutSession.SessionUrl
        );

        await _eventBus.PublishAsync(checkoutSessionCreatedEvent, cancellationToken);
    }

    private async Task<Common.Results.CheckoutSessionResult> CreateCheckoutSessionAsync(
    Payment payment,
    BookingCreatedIntegrationEvent @event,
    CancellationToken cancellationToken)
    {
        var successUrl = _paymentSettings.SuccessUrl.Replace("{CHECKOUT_SESSION_ID}", "{CHECKOUT_SESSION_ID}");
        var cancelUrl = _paymentSettings.CancelUrl;

            return await _paymentGatewayService.CreateCheckoutSessionAsync(
            payment.Id,
            @event.ProductName,
            payment.Amount,
            payment.Currency.Code,
            successUrl,
            cancelUrl,
            new Dictionary<string, string>
            {
                { _paymentSettings.PaymentIdMetadataKey, payment.Id.ToString() },
            { _paymentSettings.BookingIdMetadataKey, @event.BookingId.ToString() }
            },
            cancellationToken
            );
    }
}