using Tgs.GameBets.RapidTest.Expire.Core;

namespace Tgs.GameBets.RapidTest.Expire.DependencyInjection;

public static class ServiceRegisterExtensions
{
	public static async Task<WebApplication> UseRegister(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			try
			{
				var gameBetRapidTestRepository = scope.ServiceProvider.GetRequiredService<IGameBetsRapidTestRepository>();
				var gameProviderRepository = scope.ServiceProvider.GetRequiredService<IGameProviderRepository>();

				var gameProviderCodes = await gameProviderRepository
					.QueryAsync()
					.ToArrayAsync()
					.ConfigureAwait(false)
					?? Enumerable.Empty<string>();

				foreach (var gameProviderCode in gameProviderCodes)
				{
					await gameBetRapidTestRepository.InitializationAsync(
						gameProviderCode: gameProviderCode)
						.ConfigureAwait(false);
				}
			}
			catch
			{
				throw;
			}
		}

		return app;
	}

	public static async Task<WebApplication> UseHangfireRegisterAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			try
			{
				var sp = scope.ServiceProvider;
				var hangfireSchedule = scope.ServiceProvider.GetRequiredService<IHangfireUtensil>();
				await hangfireSchedule.JobInitAsync().ConfigureAwait(false);
			}
			catch
			{
				throw;
			}
		}

		return app;
	}
}
