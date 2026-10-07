using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PetCareBooking.Application.Common.Behaviors;
using System.Reflection;

namespace PetCareBooking.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // Register all MediatR Handlers in Application layer
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;
        }
    }
}
