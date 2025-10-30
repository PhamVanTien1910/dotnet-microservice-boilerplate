namespace IAM.Application.Common.Interfaces
{
    public interface IJwtContext
    {
        string? Jti { get; }
        DateTime? TokenExpiry { get; }
    }
}


