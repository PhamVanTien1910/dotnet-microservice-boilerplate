using BuildingBlocks.Application.Messaging.Events;

namespace Shared.IntegrationEvents
{
    public class PaymentSucceededForBookingIntegrationEvent : IntegrationEvent
    {
        public Guid BookingId { get; set; }
        public Guid ShopId { get; set; }
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ProcessedAt { get; set; }

        public PaymentSucceededForBookingIntegrationEvent(
            Guid bookingId,
            Guid shopId,
            Guid paymentId,
            decimal amount)
        {
            BookingId = bookingId;
            ShopId = shopId;
            PaymentId = paymentId;
            Amount = amount;
            ProcessedAt = DateTime.UtcNow;
        }

        // Parameterless constructor for deserialization
        public PaymentSucceededForBookingIntegrationEvent()
        {
        }
    }
}