using Camar.Application.Abstractions;
using Camar.Domain.Members;
using Camar.Domain.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Camar.Infrastructure.Persistence;

/// <summary>
/// Datos de partida de Camar Coworking para desarrollo.
/// No hace nada si ya hay recursos cargados.
/// </summary>
public static class DevelopmentSeeder
{
    // Solo para desarrollo: en un entorno real las altas pasan por /api/auth/register.
    private const string DemoPassword = "camar-demo-2026";

    public static async Task SeedAsync(
        CamarDbContext db,
        TimeProvider clock,
        IPasswordHasher passwordHasher,
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        if (await db.Resources.AnyAsync(ct))
        {
            await LogUsersAsync(db, logger, ct);
            return;
        }

        var now = clock.GetUtcNow();

        db.Resources.AddRange(
            new Resource("Sala Orion", ResourceType.MeetingRoom, 10),
            new Resource("Sala Vega", ResourceType.MeetingRoom, 6),
            new Resource("Mesa flexible 1", ResourceType.HotDesk, 1),
            new Resource("Mesa flexible 2", ResourceType.HotDesk, 1),
            new Resource("Cabina de llamadas", ResourceType.PhoneBooth, 1));

        var hash = passwordHasher.Hash(DemoPassword);

        var admin = new User("admin@camar.test", "Marta Sanz", hash, MembershipPlan.Flex, now);
        admin.PromoteToAdmin();

        db.Users.AddRange(
            admin,
            new User("ana@camar.test", "Ana Ruiz", hash, MembershipPlan.Flex, now),
            new User("luis@camar.test", "Luis Marin", hash, MembershipPlan.DayPass, now));

        await db.SaveChangesAsync(ct);

        logger?.LogInformation("Seed cargado. Contrasena de los usuarios de demo: {Password}", DemoPassword);

        await LogUsersAsync(db, logger, ct);
    }

    // Los ids se generan solos, asi que se listan para poder probar la api a mano.
    private static async Task LogUsersAsync(CamarDbContext db, ILogger? logger, CancellationToken ct)
    {
        if (logger is null)
            return;

        foreach (var user in await db.Users.AsNoTracking().ToListAsync(ct))
            logger.LogInformation("Usuario de desarrollo {Email} ({Role}) -> {Id}", user.Email, user.Role, user.Id);
    }
}
