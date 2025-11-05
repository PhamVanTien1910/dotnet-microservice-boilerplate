using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShopManagement.Application.Handlers.Commands.AddLocation;

namespace ShopManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/locations")]
    public class LocationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LocationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{shopId}")]
        [Authorize]
        public async Task<IActionResult> AddLocation([FromRoute] Guid shopId, [FromBody] AddLocationCommand command)
        {
            var commandWithShopId = command with { ShopId = shopId };
            var result = await _mediator.Send(commandWithShopId);
            return Ok(result);
        }
    }
}