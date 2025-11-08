using PaymentService.Domain.Aggregates.PaymentAggregate.Enums;

namespace PaymentService.Application.DTOs;

public record PaymentDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public PaymentStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public Guid? BookingId { get; init; }
}