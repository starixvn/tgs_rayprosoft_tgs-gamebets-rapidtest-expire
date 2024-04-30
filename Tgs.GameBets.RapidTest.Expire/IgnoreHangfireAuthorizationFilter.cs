using Hangfire.Dashboard;

namespace Tgs.GameBets.RapidTest.Expire;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
