using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Model;
using PaymentService.Domain.Aggregates.PaymentAggregate.Enums;
using PaymentService.Domain.Aggregates.PaymentAggregate.ValueObjects;

namespace PaymentService.Domain.Aggregates.PaymentAggregate.Entities;

public class Payment : AggregateRoot, ICreatedAuditable, IModifiedAuditable
{
    public Guid? BookingId { get; private set; }
    public Guid? ShopId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public Currency Currency { get; private set; } = Currency.Create("USD");
    public string? StripeSessionId { get; set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    private Payment()
    {
    } 

    private Payment(Guid userId, decimal amount, string paymentMethod, Currency currency, Guid? bookingId = null, Guid? shopId = null)
    {
        UserId = userId;
        Amount = amount > 0 ? amount : throw new ArgumentException("Amount must be greater than zero");
        PaymentMethod = !string.IsNullOrWhiteSpace(paymentMethod)
            ? paymentMethod
            : throw new ArgumentException("Payment method is required");
        Currency = currency ?? Currency.Create("USD");
        BookingId = bookingId;
        ShopId = shopId;

        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Create(Guid userId, decimal amount, string paymentMethod, Currency? currency = null,
        Guid? bookingId = null, Guid? classId = null)
        => new Payment(userId, amount, paymentMethod, currency ?? Currency.Create("USD"), bookingId, classId);

    public void MarkAsProcessing()
    {
        EnsureStatus(PaymentStatus.Pending, "processing");
        Status = PaymentStatus.Processing;
    }

    public void MarkAsSucceeded()
    {
        EnsureStatus(PaymentStatus.Processing, "succeeded");
        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string failureReason)
    {
        EnsureStatus(PaymentStatus.Processing, "failed");
        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        ProcessedAt = DateTime.UtcNow;
    }

    public void AssociateWithStripeSession(string sessionId)
    {
        StripeSessionId = sessionId;
        Status = PaymentStatus.Processing;
    }

    private void EnsureStatus(PaymentStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Payment can only be marked as {action} when in {expected} status");
    }
}