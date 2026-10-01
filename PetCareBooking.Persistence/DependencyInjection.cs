using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Persistence.Repositories;

namespace PetCareBooking.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
