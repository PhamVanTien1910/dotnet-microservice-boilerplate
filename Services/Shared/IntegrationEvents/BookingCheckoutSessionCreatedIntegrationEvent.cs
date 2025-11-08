using BuildingBlocks.Application.Messaging.Events;

namespace Shared.IntegrationEvents;

public sealed class BookingCheckoutSessionCreatedIntegrationEvent : IntegrationEvent
{
    public Guid BookingId { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string ProductName { get; init; } = "Class";
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public int NumberOfLessons { get; init; }
    public string SessionCheckoutUrl { get; init; } = string.Empty;

    public BookingCheckoutSessionCreatedIntegrationEvent(
        Guid bookingId,
        Guid userId,
        string userEmail,
        string productName,
        decimal totalAmount,
        string currency,
        int numberOfLessons,
        string sessionCheckoutUrl)
    {
        BookingId = bookingId;
        UserId = userId ;
        UserEmail = userEmail;
        ProductName = productName ?? "Product";
        TotalAmount = totalAmount;
        Currency = currency;
        NumberOfLessons = numberOfLessons;
        SessionCheckoutUrl = sessionCheckoutUrl;
    }
}


