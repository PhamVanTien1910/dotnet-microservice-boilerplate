using System.ComponentModel.DataAnnotations;

namespace NotificationService.Options
{
    public class RetryOptions
    {
        public const string SectionName = "Retry";
        [Range(0, 10)]
        public int MaxAttempts { get; init; } = 5;
        [Range(50, 60000)]
        public int InitialDelayMs { get; init; } = 500;
        [Range(50, 120000)]
        public int MaxDelayMs { get; init; } = 30000;
    }
}
