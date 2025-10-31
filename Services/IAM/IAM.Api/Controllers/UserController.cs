using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IAM.Application.Handlers.Users.Queries.GetAllUsers;
using IAM.Application.Handlers.Users.Queries.GetMyProfile;
using IAM.Application.Handlers.Users.Queries.GetUserById;
using Microsoft.AspNetCore.Authorization;
using IAM.Application.Handlers.Users.Commands.UpdateProfile;
using IAM.Application.Handlers.Users.Commands.DeleteProfile;

namespace IAM.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetCurrentUserProfile()
        {
            var query = new GetMyProfileQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var query = new GetAllUsersQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] Guid userId)
        {
            var query = new GetUserByIdQuery(userId);
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<UserResponse>> UpdateUserProfile(
            [FromBody] UpdateProfileCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("{userId:guid}")]
        [Authorize("AdminPolicy")]
        public async Task<IActionResult> DeleteUserProfile([FromRoute] Guid userId)
        {
            var command = new DeleteProfileCommand(userId);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}