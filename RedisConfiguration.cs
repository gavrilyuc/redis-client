using System.Security.Authentication;

namespace WebApi.Redis;

public class RedisConfiguration
{
	public List<string> Hosts { get; set; }

	public int ConnectTimeout { get; set; } = 5000;

	public int? DefaultDatabase { get; set; }

	public string ClientName { get; set; }

	public string Password { get; set; }

	public string User { get; set; }

	public bool Ssl { get; set; }

	public string SslHost { get; set; }

	public SslProtocols? SslProtocols { get; set; }

	public bool SkipCertificateValidation { get; set; }

	public bool AbortOnConnectFail { get; set; } = true;

	public bool CheckCertificateRevocation { get; set; }

	public Dictionary<string, CacheObjectConfiguration> Cache { get; set; } = new (StringComparer.OrdinalIgnoreCase);

	public CacheObjectConfiguration GetCacheConfig<TObject>()
		where TObject : class, ICacheableObject
	{
		var key = typeof(TObject).Name;

		return Cache.TryGetValue(key, out var val) ? val : null;
	}

	public CacheObjectConfiguration GetCacheConfig(string key)
	{
		return Cache.TryGetValue(key, out var val) ? val : null;
	}
}