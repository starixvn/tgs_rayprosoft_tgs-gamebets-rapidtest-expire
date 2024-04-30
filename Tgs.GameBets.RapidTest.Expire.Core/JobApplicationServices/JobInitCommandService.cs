using System.Runtime.InteropServices;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.GameBets.RapidTest.Expire.Core;
using Tgs.GameBets.RapidTest.Expire.Core.JobApplicationServices;

namespace Tgs.GameBets.RapidTest.Expire.Core.JobApplicationServices;

internal class JobInitCommandService(
	ILogger<JobInitCommandService> logger,
	IHangfireLock hangfireLock,
	IGameProviderRepository gameProviderRepository) : ICommandService<JobInitCommand>
{
	public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			if (!await hangfireLock.LockingAsync(
				application: typeof(JobInitCommandService).Name,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false))
			{
				logger.LogWarning("LogOn:{logOn} Message:進行初始化動作，已經有其他服務進行", DateTime.Now.ToString("o"));
				return;
			}

			using (var connection = JobStorage.Current.GetConnection())
			{
				foreach (var recurringJob in connection.GetRecurringJobs())
				{
					RecurringJob.RemoveIfExists(recurringJob.Id);
				}
			}

			var gameProviderCodes = await gameProviderRepository.QueryAsync(
				cancellationToken: cancellationToken)
				.ToArrayAsync(
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<string>();

			foreach (var gameProviderCode in gameProviderCodes)
			{
				var jobExecuteCommand = new JobExecuteCommand(
					GameProviderCode: gameProviderCode);

				RecurringJob.AddOrUpdate<IHangfireUtensil>(
					recurringJobId: gameProviderCode,
					methodCall: service => service.JobExecuteAsync(jobExecuteCommand),
					cronExpression: Cron.Minutely(),
					options: new RecurringJobOptions()
					{
						TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
					});
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
		}
		finally
		{
			_ = BackgroundJob.Schedule<IHangfireUtensil>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
		}
	}
}
