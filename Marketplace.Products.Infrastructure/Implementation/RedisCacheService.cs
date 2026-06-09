using System.Text.Json;
using Marketplace.Products.Application;
using StackExchange.Redis;

namespace Marketplace.Products.Infrastructure.Implementation;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await _db.StringGetAsync(key);
        if (cachedValue.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(cachedValue.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serializedValue);
    }

    public async Task RemoveAsync(string key) => await _db.KeyDeleteAsync(key);
}