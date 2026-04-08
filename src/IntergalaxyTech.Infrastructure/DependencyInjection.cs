using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Infrastructure.Data;
using IntergalaxyTech.Infrastructure.ExternalServices;
using IntergalaxyTech.Infrastructure.Options;
using IntergalaxyTech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IntergalaxyTech.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<PersonajeRepository>();
        services.AddScoped<SolicitudRepository>();

        services.Configure<RickAndMortyApiOptions>(configuration.GetSection(RickAndMortyApiOptions.SectionName));

        services.AddHttpClient<IRickAndMortyApiClient, RickAndMortyApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RickAndMortyApiOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                throw new InvalidOperationException("Rick And Morty API BaseUrl is not configured.");

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}
