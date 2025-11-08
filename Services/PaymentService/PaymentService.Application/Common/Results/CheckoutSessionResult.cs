namespace PaymentService.Application.Common.Results;

public class CheckoutSessionResult
{
    public required string SessionId { get; set; }
    public required string SessionUrl { get; set; }
}