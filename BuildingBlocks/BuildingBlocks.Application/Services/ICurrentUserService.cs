namespace BuildingBlocks.Application.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? OrganizationId { get; }
        string? OrganizationRole { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        string? UserName { get; }
        string? Role { get; }
        string? Jti { get; }
        DateTime? TokenExpiry { get; }
    }
}