namespace Team3.Backend.Services.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration);

    Task RemoveAsync(string key);
}