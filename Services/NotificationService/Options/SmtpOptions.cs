namespace NotificationService.Options
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FromAddress { get; set; } = default!;
    }
}
