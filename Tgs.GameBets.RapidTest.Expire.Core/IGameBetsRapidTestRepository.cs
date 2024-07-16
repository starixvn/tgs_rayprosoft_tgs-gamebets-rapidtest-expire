namespace Tgs.GameBets.RapidTest.Expire.Core;

public interface IGameBetsRapidTestRepository
{
	ValueTask SetExpireAsync(string gameProviderCode, CancellationToken cancellationToken = default);

	ValueTask InitializationAsync(string gameProviderCode, CancellationToken cancellationToken = default);
}
