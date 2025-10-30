namespace IAM.Application.Common.Interfaces
{
    public interface IJwtBlacklistService
    {
        Task BlacklistTokenAsync(string jti, TimeSpan expiry);

        Task<bool> IsTokenBlacklistedAsync(string jti);
    }
}