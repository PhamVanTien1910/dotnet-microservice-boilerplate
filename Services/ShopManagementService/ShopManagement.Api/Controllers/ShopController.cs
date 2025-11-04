using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Handlers.Commands.CreateShop;

namespace ShopManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/shops")]
    public class ShopController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShopController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateShop([FromForm] CreateShopCommand command)
        {
            var shopId = await _mediator.Send(command);
            return Ok(shopId);
        }
    }
}