namespace PaymentService.Infrastructure.Settings
{
    public class StripeSettings
    {
        public required string PublishableKey { get; set; }
        public required string SecretKey { get; set; }
        public required string WebhookSecret { get; set; }
    }
}