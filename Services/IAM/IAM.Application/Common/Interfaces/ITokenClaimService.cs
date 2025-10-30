using System.Security.Claims;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Application.Common.Interfaces
{
    public interface ITokenClaimService
    {
        Task<IEnumerable<Claim>> GetClaimsAsync(User user);
    }
}