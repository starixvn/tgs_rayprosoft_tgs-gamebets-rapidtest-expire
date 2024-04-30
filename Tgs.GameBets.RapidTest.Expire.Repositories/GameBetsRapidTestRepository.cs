using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Tgs.GameBets.RapidTest.Client;
using Tgs.GameBets.RapidTest.Expire.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Tgs.GameBets.RapidTest.Expire.Repositories;
internal class GameBetsRapidTestRepository(
	ILogger<GameBetsRapidTestRepository> logger,
	IOptions<PolicySettingsOptions> policySettingsOptions,
	IGameBetsRapidTestClient client) : IGameBetsRapidTestRepository
{
	private readonly PolicySettingsOptions _policySettingsOptions = policySettingsOptions.Value;
	public async ValueTask SetExpireAsync(string gameProviderCode, CancellationToken cancellationToken = default)
	{
		var retryCount = _policySettingsOptions.RetryCount;
		var retrySeconds = _policySettingsOptions.RetrySeconds;
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: retryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
				onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning("LogOn:{logOn} Exception:{exception} RetryTime:{retryTime} RetryCount:{retryCount}",
					DateTime.Now.ToString("o"),
					exception.Message,
					retryTime,
				retryCount));
		await retryPolicy.ExecuteAsync(() => client.GameBetRapidTestExpireAsync(
			gameProviderCode: gameProviderCode,
		   cancellationToken: cancellationToken).AsTask())
		   .ConfigureAwait(false);
	}
	public async ValueTask InitializationAsync(string gameProviderCode, CancellationToken cancellationToken = default)
	{
		var retryCount = _policySettingsOptions.RetryCount;
		var retrySeconds = _policySettingsOptions.RetrySeconds;
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: retryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
				onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning("LogOn:{logOn} Exception:{exception} RetryTime:{retryTime} RetryCount:{retryCount}",
					DateTime.Now.ToString("o"),
					exception.Message,
					retryTime,
				retryCount));
		await retryPolicy.ExecuteAsync(() => client.GameBetRapidTestInitializationAsync(
			gameProviderCode: gameProviderCode,
		   cancellationToken: cancellationToken).AsTask())
		   .ConfigureAwait(false);
	}
}
