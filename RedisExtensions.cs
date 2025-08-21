using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace WebApi.Redis;

public static class RedisExtensions
{
	public static void AddRedis(this IServiceCollection services, RedisConfiguration cfg)
	{
		if (cfg.Hosts.Count == 0)
		{
			return;
		}

		var options = new ConfigurationOptions
		{
			DefaultDatabase = cfg.DefaultDatabase,
			ClientName = cfg.ClientName,
			User = cfg.User,
			Password = cfg.Password,
			Ssl = cfg.Ssl,
			SslHost = cfg.SslHost,
			SslProtocols = cfg.SslProtocols,
			ConnectTimeout = cfg.ConnectTimeout,
			AbortOnConnectFail = cfg.AbortOnConnectFail,
			CheckCertificateRevocation = cfg.CheckCertificateRevocation
		};

		cfg.Hosts.ForEach(endpoint => options.EndPoints.Add(endpoint));

		if (cfg.SkipCertificateValidation)
		{
			options.CertificateValidation += (_, _, _, _) => true;
		}

		var multiplexer = ConnectionMultiplexer.Connect(options);

		services.AddSingleton<IConnectionMultiplexer>(multiplexer);

		services.AddSingleton(cfg);

		services.AddScoped(typeof(IRedisClient<>), typeof(RedisClient<>));
		services.AddScoped<IRedisClient, RedisClient>();
	}
}