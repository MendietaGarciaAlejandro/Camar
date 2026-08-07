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
    public static async Task SeedAsync(
        CamarDbContext db,
        TimeProvider clock,
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

        // El hash real llegara con el registro de usuarios; aqui solo hace falta que exista.
        const string placeholderHash = "sin-registro-todavia";

        db.Users.AddRange(
            new User("ana@camar.test", "Ana Ruiz", placeholderHash, MembershipPlan.Flex, now),
            new User("luis@camar.test", "Luis Marin", placeholderHash, MembershipPlan.DayPass, now));

        await db.SaveChangesAsync(ct);

        await LogUsersAsync(db, logger, ct);
    }

    // Los ids se generan solos, asi que se listan para poder probar la api a mano.
    private static async Task LogUsersAsync(CamarDbContext db, ILogger? logger, CancellationToken ct)
    {
        if (logger is null)
            return;

        foreach (var user in await db.Users.AsNoTracking().ToListAsync(ct))
            logger.LogInformation("Usuario de desarrollo {Email} -> {Id}", user.Email, user.Id);
    }
}
