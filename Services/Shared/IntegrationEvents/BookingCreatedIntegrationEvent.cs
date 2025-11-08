using BuildingBlocks.Application.Messaging.Events;

namespace Shared.IntegrationEvents;

public class BookingCreatedIntegrationEvent : IntegrationEvent
{
    public Guid BookingId { get; init; }
    public Guid ShopId { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; }
    public string ProductName { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }
    public int NumberOfLessons { get; init; }

    public BookingCreatedIntegrationEvent(Guid bookingId, Guid ShopId, Guid userId, string userEmail,
        string productName, decimal totalAmount, string currency, int numberOfLessons)
    {
        BookingId = bookingId;
        ShopId = ShopId;
        UserId = userId;
        UserEmail = userEmail;
        ProductName = productName ?? "Unknown Product";
        TotalAmount = totalAmount;
        Currency = currency;
        NumberOfLessons = numberOfLessons;
    }
}