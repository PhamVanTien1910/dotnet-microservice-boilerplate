using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Asp.Versioning;
using IAM.Application.DTOs;
using IAM.Application.Handlers.Users.Commands.ChangePassword;
using IAM.Application.Handlers.Users.Commands.EmailConfirm;
using IAM.Application.Handlers.Users.Commands.ForgotPassword;
using IAM.Application.Handlers.Users.Commands.Login;
using IAM.Application.Handlers.Users.Commands.Logout;
using IAM.Application.Handlers.Users.Commands.RefreshToken;
using IAM.Application.Handlers.Users.Commands.Register;
using IAM.Application.Handlers.Users.Commands.ResetPassword;
using Microsoft.AspNetCore.Authorization;
using IAM.Infrastructure.Configurations;

namespace IAM.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private const string RefreshTokenCookieName = "refreshToken";

        public AuthenticationController(IMediator mediator, IOptions<JwtSettings> jwtOptions)
        {
            _mediator = mediator;
            _jwtOptions = jwtOptions;
        }

        private CookieOptions CreateCookieOptions(bool isDelete = false)
        {
            var isHttps = Request.IsHttps || string.Equals(Request.Headers["X-Forwarded-Proto"], "https",
                StringComparison.OrdinalIgnoreCase);

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/",
                Expires = isDelete
                    ? DateTimeOffset.UtcNow.AddDays(-1)
                    : DateTimeOffset.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenExpirationDays)
            };
        }

        [HttpPost("register")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterCommand request)
        {
            var result = await _mediator.Send(request);
            return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
        }

        [HttpPost("login")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("confirm-email")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<EmailConfirmResponse>> ConfirmEmail([FromBody] EmailConfirmCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("logout")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<LogoutResponse>> Logout()
        {
            Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);
            var command = new LogoutCommand(refreshToken);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest? body = null)
        {
            string? refreshToken = null;

            if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieToken) &&
                !string.IsNullOrWhiteSpace(cookieToken))
            {
                refreshToken = cookieToken;
            }
            else if (!string.IsNullOrWhiteSpace(body?.RefreshToken))
            {
                refreshToken = body.RefreshToken;
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Refresh token is missing.",
                    Instance = HttpContext.Request.Path
                });

            var command = new RefreshTokenCommand(refreshToken);
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPut("change-password")]
        [Authorize]
        [MapToApiVersion(1.0)]
        public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}