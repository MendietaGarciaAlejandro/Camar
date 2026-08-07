using Camar.Application.Abstractions;
using Camar.Infrastructure.Persistence;
using Camar.Infrastructure.Persistence.Repositories;
using Camar.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Camar.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la persistencia. Recibe la cadena de conexion en vez de IConfiguration
    /// para que Infrastructure no dependa del sistema de configuracion de la Api.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<CamarDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBlockedDayRepository, BlockedDayRepository>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
