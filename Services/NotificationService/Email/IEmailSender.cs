namespace NotificationService.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string html, CancellationToken cancellationToken = default);
    }
}
