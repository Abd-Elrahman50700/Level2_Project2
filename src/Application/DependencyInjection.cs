using System.Reflection;
using Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR and Pipeline Behaviors in correct order:
        // 1. Logging (outermost: logs request name and measures execution time)
        // 2. Validation (innermost: validates request and stops execution on failure)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register all FluentValidation validators in assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
