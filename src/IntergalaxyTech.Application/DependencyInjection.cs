using System.Reflection;
using FluentValidation;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IntergalaxyTech.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPersonajeService, PersonajeService>();
        services.AddScoped<ISolicitudService, SolicitudService>();
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
