using System.Globalization;
using System.Text.Json;
using StackExchange.Redis;
#nullable disable

namespace WebApi.Redis;

internal sealed class RedisClient(IConnectionMultiplexer multiplexer, RedisConfiguration cfg) : IRedisClient
{
	private static readonly Task<DateTime?> NullDateTime = Task.FromResult<DateTime?>(null);

	public async Task<string> GetRaw(string key)
	{
		var cacheConfig = cfg.GetCacheConfig(key);
		if (cacheConfig is { Enabled: false })
		{
			return null;
		}

		var db = multiplexer.GetDatabase();

		var data = await db.StringGetAsync(key).ConfigureAwait(false);

		return !data.HasValue ? null : data.ToString();
	}

	public Task AddRaw(string key, string data, TimeSpan? expiry = null)
	{
		var cacheConfig = cfg.GetCacheConfig(key);
		if (cacheConfig is { Enabled: false })
		{
			return Task.CompletedTask;
		}

		var db = multiplexer.GetDatabase();

		if (cacheConfig is null)
		{
			return db.StringSetAsync(key, data, expiry);
		}

		var k = string.Format(CultureInfo.InvariantCulture, cacheConfig.KeyName, key);
		return db.StringSetAsync(k, data, expiry ?? cacheConfig.Expiry);
	}

	public Task<string> GetOrAddRaw(
		string key,
		Func<CancellationToken, Task<string>> addFunc,
		CancellationToken token = default,
		TimeSpan? expiry = null)
	{
		var cacheConfig = cfg.GetCacheConfig(key);
		if (cacheConfig is { Enabled: false })
		{
			return addFunc(token);
		}

		var db = multiplexer.GetDatabase();
		if (cacheConfig is null)
		{
			return db.GetOrCreate(key, addFunc, expiry, token);
		}

		var k = string.Format(CultureInfo.InvariantCulture, cacheConfig.KeyName, key);
		return db.GetOrCreate(k, addFunc, expiry ?? cacheConfig.Expiry, token);
	}

	public Task Remove(string key)
	{
		var cacheConfig = cfg.GetCacheConfig(key);
		if (cacheConfig is { Enabled: false })
		{
			return Task.CompletedTask;
		}

		var db = multiplexer.GetDatabase();

		return db.KeyDeleteAsync(key);
	}

	public Task<DateTime?> Expires(string key)
	{
		var cacheConfig = cfg.GetCacheConfig(key);
		if (cacheConfig is { Enabled: false })
		{
			return NullDateTime;
		}

		var db = multiplexer.GetDatabase();

		return db.KeyExpireTimeAsync(key);
	}
}

internal sealed class RedisClient <T>(IConnectionMultiplexer multiplexer, RedisConfiguration cfg) : IRedisClient<T>
	where T : class, ICacheableObject
{
	private readonly CacheObjectConfiguration _cacheCfg = cfg.GetCacheConfig<T>();

	public async Task<T> Get(string key)
	{
		if (_cacheCfg is { Enabled: false })
		{
			return null;
		}

		var db = multiplexer.GetDatabase();
		var data = await db.StringGetAsync(key).ConfigureAwait(false);

		if (!data.HasValue)
		{
			return null;
		}

		return JsonSerializer.Deserialize<T>(data.ToString());
	}

	public Task Add(string key, T data, TimeSpan? expiry = null)
	{
		if (_cacheCfg is { Enabled: false })
		{
			return Task.CompletedTask;
		}

		var db = multiplexer.GetDatabase();
		var serializedData = JsonSerializer.Serialize(data);

		if (_cacheCfg is null)
		{
			return db.StringSetAsync(key, serializedData, expiry);
		}

		var k = string.Format(CultureInfo.InvariantCulture, _cacheCfg.KeyName, key);
		return db.StringSetAsync(k, serializedData, expiry ?? _cacheCfg.Expiry);
	}

	public Task<T> GetOrAdd(
		string key,
		Func<CancellationToken, Task<T>> addFunc,
		CancellationToken token = default,
		TimeSpan? expiry = null)
	{
		if (_cacheCfg is { Enabled: false })
		{
			return addFunc(token);
		}

		var db = multiplexer.GetDatabase();
		if (_cacheCfg is null)
		{
			return db.GetOrCreate(key, addFunc, expiry, token);
		}

		var k = string.Format(CultureInfo.InvariantCulture, _cacheCfg.KeyName, key);
		return db.GetOrCreate(k, addFunc, expiry ?? _cacheCfg.Expiry, token);
	}

	public Task<List<T>> GetOrAddList(
		string key,
		Func<CancellationToken, Task<List<T>>> addFunc,
		CancellationToken token = default,
		TimeSpan? expiry = null)
	{
		if (_cacheCfg is { Enabled: false })
		{
			return addFunc(token);
		}

		var db = multiplexer.GetDatabase();
		if (_cacheCfg is null)
		{
			return db.GetOrCreateList(key, addFunc, expiry, token);
		}

		var k = string.Format(CultureInfo.InvariantCulture, _cacheCfg.KeyName, key);
		return db.GetOrCreateList(k, addFunc, expiry ?? _cacheCfg.Expiry, token);
	}

	public Task Remove(string key)
	{
		if (_cacheCfg is { Enabled: false })
		{
			return Task.CompletedTask;
		}

		var db = multiplexer.GetDatabase();

		return db.KeyDeleteAsync(key);
	}
}