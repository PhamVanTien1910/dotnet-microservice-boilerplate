using NotificationService.Options;
using Microsoft.Extensions.Options;
using System;
using System.Text;

namespace NotificationService.Email
{
    public class EmailComposer
    {
        private readonly EmailOptions _emailOptions;
        private readonly PortalKeySettings _portalKeySettings;
        private readonly ITemplateRenderer _templateRenderer;

        public EmailComposer(IOptions<EmailOptions> emailOptions, IOptions<PortalKeySettings> portalKeySettings, ITemplateRenderer templateRenderer)
        {
            _emailOptions = emailOptions.Value;
            _portalKeySettings = portalKeySettings.Value;
            _templateRenderer = templateRenderer;
        }

        public async Task<(string Subject, string Html)> ComposeVerificationAsync(string email, string firstName, string lastName, string token, DateTimeOffset expiresAt, CancellationToken ct)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Confirmation token cannot be null or empty", nameof(token));
            }

            var url = _emailOptions.VerificationUrlTemplate
                .Replace("{token}", Uri.EscapeDataString(token))
                .Replace("{email}", Uri.EscapeDataString(email))
                .Replace("{firstName}", Uri.EscapeDataString(firstName ?? string.Empty))
                .Replace("{lastName}", Uri.EscapeDataString(lastName ?? string.Empty));
            var model = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["FirstName"] = firstName ?? string.Empty,
                ["LastName"] = lastName ?? string.Empty,
                ["ConfirmationLink"] = url,
                ["ExpiresAt"] = expiresAt.ToString("u")
            };
            var html = await _templateRenderer.RenderAsync("verification.html", model, ct);
            return ("Verify your email", html);
        }

        public async Task<(string Subject, string Html)> ComposeResetAsync(string email, string role, string token, DateTimeOffset expiresAt, CancellationToken ct)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Reset token cannot be null or empty", nameof(token));
            }

            string portalValue = role?.ToLowerInvariant() switch
            {
                "admin" => _portalKeySettings.Admin,
                "provider" => _portalKeySettings.Provider,
                "parent" => _portalKeySettings.Parent,
                _ => _portalKeySettings.Parent // Default to parent portal
            };

            var url = _emailOptions.ResetUrlTemplate
            .Replace("{token}", Uri.EscapeDataString(token))
            .Replace("{portal}", Uri.EscapeDataString(portalValue));

            var model = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["ResetLink"] = url,
                ["ExpiresAt"] = expiresAt.ToString("u")
            };

            var html = await _templateRenderer.RenderAsync("reset.html", model, ct);
            return ("Reset your password", html);
        }

        public async Task<(string Subject, string Html)> ComposeInvitationAsync(string email, string token, string callbackUrl, DateTimeOffset expiresAt, Guid invitationId, CancellationToken ct)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Invitation token cannot be null or empty", nameof(token));
            }

            if (string.IsNullOrEmpty(callbackUrl))
            {
                throw new ArgumentException("Callback URL cannot be null or empty", nameof(callbackUrl));
            }

            // Build the invitation URL with token
            var invitationUrl = callbackUrl.Contains("?")
                ? $"{callbackUrl}&token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}"
                : $"{callbackUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

            var model = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["InvitationLink"] = invitationUrl,
                ["ExpiresAt"] = expiresAt.ToString("f"), // Full date/time pattern
                ["Token"] = token,
                ["InvitationId"] = invitationId.ToString()
            };

            var html = await _templateRenderer.RenderAsync("invitation.html", model, ct);
            return ("Staff Invitation - Join Our Team", html);
        }

        public async Task<(string Subject, string Html)> ComposeDefaultParentAccountAsync(string email, string firstName, string lastName, string temporaryPassword, CancellationToken ct)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(temporaryPassword))
            {
                throw new ArgumentException("Temporary password cannot be null or empty", nameof(temporaryPassword));
            }

            var loginUrl = _emailOptions.LoginUrlTemplate; // Should be configured in appsettings

            var model = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["FirstName"] = firstName ?? string.Empty,
                ["LastName"] = lastName ?? string.Empty,
                ["TemporaryPassword"] = temporaryPassword,
                ["LoginUrl"] = loginUrl ?? "#"
            };

            var html = await _templateRenderer.RenderAsync("default-parent-account.html", model, ct);
            return ("Welcome! Your Parent Account Has Been Created", html);
        }

        public async Task<(string Subject, string Html)> ComposeLessonCancelledAsync(string parentEmail, Guid classId, Guid bookedLessonId, DateTime cancelledAtUtc, string programName, DateTime lessonStartUtc, DateTime lessonEndUtc, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(parentEmail))
            {
                throw new ArgumentException("Parent email cannot be null or empty", nameof(parentEmail));
            }

            var model = new Dictionary<string, string>
            {
                ["ParentEmail"] = parentEmail,
                ["ClassId"] = classId.ToString(),
                ["BookedLessonId"] = bookedLessonId.ToString(),
                ["CancelledAt"] = cancelledAtUtc.ToString("u"),
                ["ProgramName"] = programName ?? "Class",
                ["LessonStart"] = lessonStartUtc.ToString("u"),
                ["LessonEnd"] = lessonEndUtc.ToString("u")
            };

            var html = await _templateRenderer.RenderAsync("lesson-cancelled.html", model, ct);
            return ("Lesson Cancellation Confirmation", html);
        }

        public async Task<(string Subject, string Html)> ComposeBookingCheckoutAsync(
            string parentEmail,
            Guid bookingId,
            string programName,
            decimal totalAmount,
            string currency,
            int numberOfLessons,
            string checkoutUrl,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(parentEmail))
                throw new ArgumentException("Parent email cannot be null or empty", nameof(parentEmail));
            if (string.IsNullOrWhiteSpace(checkoutUrl))
                throw new ArgumentException("Checkout URL cannot be null or empty", nameof(checkoutUrl));

            var model = new Dictionary<string, string>
            {
                ["ParentEmail"] = parentEmail,
                ["BookingId"] = bookingId.ToString(),
                ["ProgramName"] = programName ?? "Class",
                ["TotalAmount"] = totalAmount.ToString("0.00"),
                ["Currency"] = currency,
                ["NumberOfLessons"] = numberOfLessons.ToString(),
                ["CheckoutUrl"] = checkoutUrl
            };

            var html = await _templateRenderer.RenderAsync("booking-checkout.html", model, ct);
            return ("Complete Your Booking - Proceed to Checkout", html);
        }

        public async Task<(string Subject, string Html)> ComposeBookingPaidReceiptAsync(
            string parentEmail,
            Guid bookingId,
            Guid paymentId,
            string programName,
            decimal totalAmount,
            string currency,
            int numberOfLessons,
            DateTime paidAt,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(parentEmail))
                throw new ArgumentException("Parent email cannot be null or empty", nameof(parentEmail));

            var model = new Dictionary<string, string>
            {
                ["ParentEmail"] = parentEmail,
                ["BookingId"] = bookingId.ToString(),
                ["PaymentId"] = paymentId.ToString(),
                ["ProgramName"] = programName ?? "Class",
                ["TotalAmount"] = totalAmount.ToString("0.00"),
                ["Currency"] = currency,
                ["NumberOfLessons"] = numberOfLessons.ToString(),
                ["PaidAt"] = paidAt.ToString("u")
            };

            var html = await _templateRenderer.RenderAsync("booking-paid.html", model, ct);
            return ("Payment Confirmed - Receipt", html);
        }
    }
}
