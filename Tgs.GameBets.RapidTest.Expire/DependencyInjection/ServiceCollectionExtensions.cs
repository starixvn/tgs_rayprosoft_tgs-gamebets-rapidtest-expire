using Microsoft.EntityFrameworkCore;
using Tgs.GameBets.RapidTest.Expire;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameBetsRapidTestExpire(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
		Action<RedisOptions, IServiceProvider> redisOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddGameBetsRapidTestExpireCore()
		.AddGameBetsRapidTestExpireRepositories(
			dbContextOptions: dbContextOptions,
			redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
