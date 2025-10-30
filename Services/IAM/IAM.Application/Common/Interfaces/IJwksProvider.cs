using IAM.Application.Common.Models;

namespace IAM.Application.Common.Interfaces
{
    public interface IJwksProvider
    {
        JwksDocument? GetJwksDocument();
    }
}


