using NSubstitute;
using Tgs.GameBets.RapidTest.Expire.Core.ApplicationServices;

namespace Tgs.GameBets.RapidTest.Expire.Core.UnitTest.ApplicationServices;

public class GameBetRapidTestExpireCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var fakeRepository = Substitute.For<IGameBetsRapidTestRepository>();

		var command = new GameBetRapidTestExpireCommand(
			GameProviderCode: "KY");

		var sut = new GameBetRapidTestExpireCommandService(
			fakeRepository);

		await sut.ExecuteAsync(command: command);

		_ = fakeRepository
			.Received(1)
			.SetExpireAsync(
			gameProviderCode: Arg.Is(command.GameProviderCode),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
