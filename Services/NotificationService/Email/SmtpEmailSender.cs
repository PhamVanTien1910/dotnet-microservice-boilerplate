using NotificationService.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;

namespace NotificationService.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtp;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly IAsyncPolicy _retryPolicy;

        public SmtpEmailSender(
            IOptions<SmtpOptions> smtp, 
            ILogger<SmtpEmailSender> logger)
        {
            _smtp = smtp.Value;
            _logger = logger;
            
            // Create a simple retry policy for email sending
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(outcome, "Email sending failed, retry {RetryCount} after {Delay}ms", 
                            retryCount, timespan.TotalMilliseconds);
                    });
        }

        public async Task SendAsync(string to, string subject, string html, CancellationToken ct = default)
        {
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var msg = new MimeMessage();
                    msg.From.Add(new MailboxAddress("Boilerplate", _smtp.FromAddress));
                    msg.To.Add(MailboxAddress.Parse(to));
                    msg.Subject = subject;
                    msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

                    using var client = new SmtpClient();
                    await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls, ct);
                    if (!string.IsNullOrWhiteSpace(_smtp.Username))
                        await client.AuthenticateAsync(_smtp.Username, _smtp.Password, ct);
                    await client.SendAsync(msg, ct);
                    await client.DisconnectAsync(true, ct);
                });

                _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
                throw new InvalidOperationException($"Failed to send email to {to}: {ex.Message}", ex);
            }
        }
    }
}
