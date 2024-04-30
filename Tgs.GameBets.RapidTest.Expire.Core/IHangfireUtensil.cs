using Tgs.GameBets.RapidTest.Expire.Core.JobApplicationServices;

namespace Tgs.GameBets.RapidTest.Expire.Core;

public interface IHangfireUtensil
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
