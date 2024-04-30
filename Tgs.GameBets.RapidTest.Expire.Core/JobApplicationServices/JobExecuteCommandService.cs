using Microsoft.Extensions.Logging;
using Tgs.GameBets.RapidTest.Expire.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameBets.RapidTest.Expire.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var message = new GameBetsRapidTestExpireRequest
		{
			GameProviderCode = command.GameProviderCode
		};

		logger.LogInformation("LogOn:{logOn} Publish Message:{message}", DateTime.Now.ToString("u"), message);

		return messageSender.PublishAsync(
			subject: Common.GameBetsRapidTestExpireNatsConst.GameBetsRapidTestExpireRequest,
			data: message,
			cancellationToken: cancellationToken);
	}
}
