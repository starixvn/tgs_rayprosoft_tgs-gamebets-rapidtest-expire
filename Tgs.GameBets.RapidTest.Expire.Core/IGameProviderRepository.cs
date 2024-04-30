namespace Tgs.GameBets.RapidTest.Expire.Core;

public interface IGameProviderRepository
{
	IAsyncEnumerable<string> QueryAsync(CancellationToken cancellationToken = default);
}
