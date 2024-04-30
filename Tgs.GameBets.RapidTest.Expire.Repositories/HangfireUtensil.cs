using Tgs.GameBets.RapidTest.Expire.Core;
using Tgs.GameBets.RapidTest.Expire.Core.JobApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameBets.RapidTest.Expire.Repositories;

internal class HangfireUtensil(
	ICommandService<JobCheckCommand> jobCheckService,
	ICommandService<JobExecuteCommand> jobExecuteService,
	ICommandService<JobInitCommand> jobInitservice) : IHangfireUtensil
{
	public Task JobCheckAsync()
		=> jobCheckService.ExecuteAsync(new()).AsTask();

	public Task JobExecuteAsync(JobExecuteCommand source)
		=> jobExecuteService.ExecuteAsync(source).AsTask();

	public Task JobInitAsync()
		=> jobInitservice.ExecuteAsync(new()).AsTask();
}
