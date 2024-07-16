using Tgs.GameBets.RapidTest.Expire.Core.ApplicationServices;

using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameBets.RapidTest.Expire.Messages;

public class RapidTestExpireMessageHandler(
	ILogger<RapidTestExpireMessageHandler> logger,
	ICommandService<GameBetRapidTestExpireCommand> service) : IMessageHandler<Common.Protos.GameBetsRapidTestExpireRequest>
{
	public ValueTask HandleAsync(string subject, Common.Protos.GameBetsRapidTestExpireRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new GameBetRapidTestExpireCommand(
				GameProviderCode: data.GameProviderCode),
			cancellationToken: cancellationToken);
	}
}
