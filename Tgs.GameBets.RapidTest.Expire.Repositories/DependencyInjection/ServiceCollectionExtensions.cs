using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Tgs.GameBets.RapidTest.Expire.Core;
using Tgs.GameBets.RapidTest.Expire.Entities;
using Tgs.GameBets.RapidTest.Expire.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameBetsRapidTestExpireRepositories(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
		.AddDbContext<TgsContext>(dbContextOptions)
		.AddTransient<IGameProviderRepository, GameProviderRepository>()
		.AddTransient<IGameBetsRapidTestRepository, GameBetsRapidTestRepository>()
		.AddTransient<IHangfireLock>(sp => ActivatorUtilities.CreateInstance<HangfireLock>(sp, redisDatabaseResolver(sp)))
		.AddTransient<IHangfireUtensil, HangfireUtensil>();
}
