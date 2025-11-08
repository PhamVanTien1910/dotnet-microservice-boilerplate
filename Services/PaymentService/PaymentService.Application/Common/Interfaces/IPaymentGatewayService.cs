using PaymentService.Application.Common.Results;

namespace PaymentService.Application.Common.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<CheckoutSessionResult> CreateCheckoutSessionAsync(Guid paymentId, string orderName, decimal amount, string currency,
            string successUrl, string cancelUrl, Dictionary<string, string> metaData, CancellationToken cancellationToken = default);

        Task<CheckoutSessionInfoResult> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}