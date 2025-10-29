namespace BuildingBlocks.Application.Services
{
     public interface ICacheService
    {
        Task SetValueAsync(string key, string value, TimeSpan expiration);
        Task<string?> GetValueAsync(string key);
        Task<bool> CheckExistsAsync(string key);
        Task RemoveAsync(string key);
    }
}