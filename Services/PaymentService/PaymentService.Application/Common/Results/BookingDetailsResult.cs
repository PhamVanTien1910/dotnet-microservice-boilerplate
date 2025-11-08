namespace PaymentService.Application.Common.Results;

public record BookingDetailsResult
{
    public Guid BookingId { get; init; }
    public Guid ClassId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int NumberOfLessons { get; init; }
}
