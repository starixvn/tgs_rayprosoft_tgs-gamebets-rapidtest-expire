using System.Text.Json.Nodes;
using StackExchange.Redis;
using Tgs.GameBets.RapidTest.Expire.Core;

namespace Tgs.GameBets.RapidTest.Expire.Repositories;

internal class HangfireLock(
	IDatabase database) : IHangfireLock
{
	public static string GetKey(string application)
		=> $"gamebets-rapidtest-expire-lock:hangfire:{application}";

	public Task<bool> LockingAsync(string application, CancellationToken cancellationToken = default)
		=> database.StringSetAsync(
			key: GetKey(application),
			value: new JsonObject
			{
				["MachineName"] = Environment.MachineName,
				["Application"] = application,
				["LabelTime"] = DateTimeOffset.Now
			}.ToJsonString(),
			expiry: TimeSpan.FromSeconds(59),
			when: When.NotExists);
}
