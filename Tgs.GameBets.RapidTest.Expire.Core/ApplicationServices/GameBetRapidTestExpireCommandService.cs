using Tgs.ServiceUtility;

namespace Tgs.GameBets.RapidTest.Expire.Core.ApplicationServices;

internal class GameBetRapidTestExpireCommandService(
	IGameBetsRapidTestRepository repository) : ICommandService<GameBetRapidTestExpireCommand>
{
	public ValueTask ExecuteAsync(GameBetRapidTestExpireCommand command, CancellationToken cancellationToken = default)
		=> repository.SetExpireAsync(command.GameProviderCode, cancellationToken);
}
