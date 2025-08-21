namespace WebApi.Redis;

public class CacheObjectConfiguration
{
	public bool Enabled { get; set; } = true;

	public string KeyName { get; set; }

	public TimeSpan? Expiry { get; set; }
}