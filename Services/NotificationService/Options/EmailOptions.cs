namespace NotificationService.Options
{
    public class EmailOptions
    {
        public const string SectionName = "Email";
        public string FromName { get; set; } = "E-Learning";
        public string VerificationUrlTemplate { get; set; } = default!;
        public string ResetUrlTemplate { get; set; } = default!;
        public string LoginUrlTemplate { get; set; } = default!;
        public string? TemplatePath { get; set; }
    }
}
