namespace PaymentService.Application.Common.Results;

public record CheckoutSessionInfoResult
{
    public string Status { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}

