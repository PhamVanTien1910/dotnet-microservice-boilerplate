namespace PaymentService.Application.Settings
{
    public class PaymentSettings
    {
        public required string SuccessUrl { get; set; }
        public required string CancelUrl { get; set; }
        public required string PaymentMethodType { get; set; } = "card";
        public required string SessionMode { get; set; } = "payment";
        public required string PaymentIdMetadataKey { get; set; } = "paymentId";
        public required string BookingIdMetadataKey { get; set; } = "bookingId";
        public required string SessionIdQueryParameter { get; set; } = "session_id";
    }
}
