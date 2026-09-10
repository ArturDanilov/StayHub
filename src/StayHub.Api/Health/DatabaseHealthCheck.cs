using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StayHub.Dal.Data;

namespace StayHub.Api.Health;

public sealed class DatabaseHealthCheck(
    IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StayHubDbContext>();

        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("The database is reachable.")
                : HealthCheckResult.Unhealthy("The database is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "The database readiness check failed.",
                exception);
        }
    }
}
