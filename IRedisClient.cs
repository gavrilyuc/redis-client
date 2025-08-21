namespace WebApi.Redis;

public interface IRedisClient<T> where T : class, ICacheableObject
{
	Task<T> Get(string key);

	Task Add(string key, T data, TimeSpan? expiry = null);

	Task<T> GetOrAdd(
		string key,
		Func<CancellationToken, Task<T>> addFunc,
		CancellationToken token = default,
		TimeSpan? expiry = null);

	Task<List<T>> GetOrAddList(
		string key,
		Func<CancellationToken, Task<List<T>>> addFunc,
		CancellationToken token = default,
		TimeSpan? expiry = null);

	Task Remove(string key);
}

public interface IRedisClient
{
	Task<string> GetRaw(string key);

	Task AddRaw(string key, string data, TimeSpan? expiry = null);

	Task<string> GetOrAddRaw(
		string key,
		Func<CancellationToken, Task<string>> addFunc,
		CancellationToken token = default,
		TimeSpan? expiry = null);

	Task Remove(string key);

	Task<DateTime?> Expires(string key);
}
