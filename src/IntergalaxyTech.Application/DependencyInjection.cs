using Microsoft.Extensions.DependencyInjection;

namespace IntergalaxyTech.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registro de servicios de aplicación (MediatR, FluentValidation, etc.)
        return services;
    }
}
