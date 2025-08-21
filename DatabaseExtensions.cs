using StackExchange.Redis;
using System.Text.Json;

namespace WebApi.Redis;

internal static class DatabaseExtensions
{
	public static async Task<string> GetOrCreate(
		this IDatabase database,
		string key,
		Func<CancellationToken, Task<string>> createFunc,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default)
	{
		var existingValue = await database.StringGetAsync(key);

		if (existingValue.HasValue)
		{
			return existingValue;
		}

		var value = await createFunc.Invoke(cancellationToken);

		await database.StringSetAsync(key, value, expiration);

		return value;
	}

	public static async Task<T> GetOrCreate<T>(
		this IDatabase database,
		string key,
		Func<CancellationToken, Task<T>> createFunc,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default) where T : class
	{
		var serializedValue = await database.StringGetAsync(key);

		if (serializedValue.HasValue)
		{
			return JsonSerializer.Deserialize<T>(serializedValue);
		}

		var value = await createFunc.Invoke(cancellationToken);

		var jsonValue = JsonSerializer.Serialize(value);

		await database.StringSetAsync(key, jsonValue, expiration);

		return value;
	}

	public static async Task<List<T>> GetOrCreateList<T>(
		this IDatabase database,
		string key,
		Func<CancellationToken, Task<List<T>>> createFunc,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default) where T : class
	{
		var serializedValue = await database.StringGetAsync(key);

		if (serializedValue.HasValue)
		{
			return JsonSerializer.Deserialize<List<T>>(serializedValue);
		}

		var value = await createFunc.Invoke(cancellationToken);

		var jsonValue = JsonSerializer.Serialize(value);
		await database.StringSetAsync(key, jsonValue, expiration);

		return value;
	}
}