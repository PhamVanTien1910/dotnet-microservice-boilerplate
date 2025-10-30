using IAM.Application.Common.Interfaces;
using IAM.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAM.Api.Controllers
{
    [ApiController]
    [Route("api/.well-known")]
    public class WellKnownController : ControllerBase
    {
        private readonly IJwksProvider _jwksProvider;

        public WellKnownController(IJwksProvider jwksProvider)
        {
            _jwksProvider = jwksProvider;
        }

        [HttpGet("jwks.json")]
        [AllowAnonymous]
        public ActionResult<JwksDocument> GetJwks()
        {
            var jwks = _jwksProvider.GetJwksDocument();
            if (jwks is null || !jwks.Keys.Any())
            {
                return NotFound(new { error = "JWKS not available" });
            }

            return Ok(jwks);
        }
    }
}
