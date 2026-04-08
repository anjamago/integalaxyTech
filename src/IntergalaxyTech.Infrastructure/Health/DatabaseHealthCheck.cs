using IntergalaxyTech.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IntergalaxyTech.Infrastructure.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
        if (canConnect)
        {
            return HealthCheckResult.Healthy("La base de datos SQLite está accesible.");
        }

        return HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
    }
}
