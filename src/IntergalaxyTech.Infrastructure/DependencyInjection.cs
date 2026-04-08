using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Infrastructure.Data;
using IntergalaxyTech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntergalaxyTech.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=intergalaxy.db"));

        // Registro de Repositorios
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<PersonajeRepository>();
        services.AddScoped<SolicitudRepository>();

        // Registro de clientes HTTP, etc.
        return services;
    }
}
