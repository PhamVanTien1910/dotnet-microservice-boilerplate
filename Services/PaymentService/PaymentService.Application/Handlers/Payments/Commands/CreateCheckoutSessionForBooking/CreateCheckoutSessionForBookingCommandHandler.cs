using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using Microsoft.Extensions.Options;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;
using PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;
using PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;
using PaymentService.Domain.Aggregates.PaymentAggregate.ValueObjects;
using PaymentService.Application.Settings;

namespace PaymentService.Application.Handlers.Payments.Commands.CreateCheckoutSessionForBooking;

public class CreateCheckoutSessionForBookingCommandHandler
    : ICommandHandler<CreateCheckoutSessionForBookingCommand, string>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PaymentSettings _paymentSettings;

    public CreateCheckoutSessionForBookingCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGatewayService paymentGatewayService,
        IUnitOfWork unitOfWork,
        IOptions<PaymentSettings> paymentSettings)
    {
        _paymentRepository = paymentRepository;
        _paymentGatewayService = paymentGatewayService;
        _unitOfWork = unitOfWork;
        _paymentSettings = paymentSettings.Value;
    }

    public async Task<string> Handle(CreateCheckoutSessionForBookingCommand request,
        CancellationToken cancellationToken)
    {
        // Try to get existing payment (for idempotency)
        var payment = await _paymentRepository.GetBySpecAsync(
            new PaymentByBookingIdSpecification(request.BookingId),
            cancellationToken);

        if (payment == null)
        {
            payment = Payment.Create(
                request.UserId,
                request.Amount,
                _paymentSettings.PaymentMethodType,
                Currency.USD,
                request.BookingId);

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var productName = request.ProductName;

        var successUrl = _paymentSettings.SuccessUrl.Replace("{CHECKOUT_SESSION_ID}", "{CHECKOUT_SESSION_ID}");
        var cancelUrl = _paymentSettings.CancelUrl;

        var checkoutSession = await _paymentGatewayService.CreateCheckoutSessionAsync(
            payment.Id,
            productName,
            payment.Amount,
            payment.Currency.Code,
            successUrl,
            cancelUrl,
            new Dictionary<string, string>
            {
                { _paymentSettings.PaymentIdMetadataKey, payment.Id.ToString() },
                { _paymentSettings.BookingIdMetadataKey, request.BookingId.ToString() }
            },
            cancellationToken
        );

        payment.AssociateWithStripeSession(checkoutSession.SessionId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return checkoutSession.SessionUrl;
    }
}