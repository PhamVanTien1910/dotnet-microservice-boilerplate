using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IAM.Application.DTOs;
using IAM.Application.Handlers.Users.Commands.BlacklistToken;
using MediatR;

namespace IAM.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/tokens")]
    public class TokenController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TokenController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("blacklist")]
        public async Task<ActionResult<BlacklistTokenResponse>> BlacklistToken(
            [FromBody] BlacklistTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
    }
}