using Tgs.GameBets.RapidTest.Expire.Core;
using Tgs.GameBets.RapidTest.Expire.Entities;

namespace Tgs.GameBets.RapidTest.Expire.Repositories;

internal class GameProviderRepository(
	TgsContext context) : IGameProviderRepository
{
	public IAsyncEnumerable<string> QueryAsync(CancellationToken cancellationToken = default)
		=> context.GameProviders
		.Where(gp => gp.Status != 1 && gp.TransferWallet)
		.Select(gp => gp.Code)
		.ToAsyncEnumerable();
}
