// using Microsoft.Extensions.Options;
// using PaymentService.Application.Common.Interfaces;
// using PaymentService.Application.Common.Results;
// using PaymentService.Application.Settings;
// using PaymentService.Infrastructure.Settings;
// using Stripe;
// using Stripe.Checkout;

// namespace PaymentService.Infrastructure.Services
// {
//     public class StripeService : IPaymentGatewayService
//     {
//         private readonly StripeSettings _stripeSettings;
//         private readonly PaymentSettings _paymentSettings;

//         public StripeService(IOptions<StripeSettings> stripeSettings, IOptions<PaymentSettings> paymentSettings)
//         {
//             _stripeSettings = stripeSettings.Value;
//             _paymentSettings = paymentSettings.Value;
//             StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
//         }

//         public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(Guid paymentId, string programName,
//             decimal amount, string currency, string successUrl, string cancelUrl, Dictionary<string, string> metaData,
//             CancellationToken cancellationToken = default)
//         {
//             var options = new SessionCreateOptions
//             {
//             PaymentMethodTypes =
//             [
//             _paymentSettings.PaymentMethodType
//             ],
//             LineItems =
//             [
//             new SessionLineItemOptions
//             {
//             PriceData = new SessionLineItemPriceDataOptions
//             {
//             UnitAmount = (long)(amount * 100),
//             Currency = currency.ToLowerInvariant(), // Ensure lowercase
//             ProductData = new SessionLineItemPriceDataProductDataOptions
//             {
//             Name = programName,
//             },
//             },
//             Quantity = 1,
//             }
//             ],
//             Mode = _paymentSettings.SessionMode,
//             SuccessUrl = successUrl,
//             CancelUrl = cancelUrl,
//             Metadata = new Dictionary<string, string>
//             {
//             { _paymentSettings.PaymentIdMetadataKey, paymentId.ToString() }
//             }
//             };

//             var service = new SessionService();
//             var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

//             return new CheckoutSessionResult
//             {
//                 SessionId = session.Id,
//                 SessionUrl = session.Url
//             };
//         }

//         public async Task<CheckoutSessionInfoResult> GetCheckoutSessionAsync(string sessionId,
//             CancellationToken cancellationToken = default)
//         {
//             var service = new SessionService();
//             var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);

//             return new CheckoutSessionInfoResult
//             {
//                 Status = session.Status,
//                 PaymentStatus = session.PaymentStatus,
//                 CustomerEmail = session.CustomerDetails?.Email,
//                 Metadata = session.Metadata ?? new Dictionary<string, string>()
//             };
//         }
//     }
// }