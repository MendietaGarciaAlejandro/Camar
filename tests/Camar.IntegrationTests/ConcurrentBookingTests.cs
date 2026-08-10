using Camar.Application.Reservations;
using Camar.Domain.Common;
using Camar.Domain.Members;
using Camar.Domain.Reservations;
using Camar.Domain.Resources;
using Camar.Infrastructure.Persistence;
using Camar.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace Camar.IntegrationTests;

/// <summary>
/// La prueba de fuego del proyecto: varias peticiones peleando por el mismo hueco.
/// </summary>
public class ConcurrentBookingTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    // Lunes 12/01/2026. La reserva es el miercoles 14, dentro de la ventana del plan Flex.
    private static readonly DateTimeOffset Ahora = new(2026, 1, 12, 8, 0, 0, TimeSpan.Zero);

    private static Period HuecoDisputado() => new(
        new DateTimeOffset(2026, 1, 14, 10, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, 14, 11, 0, 0, TimeSpan.Zero));

    private ReservationService CreateService(CamarDbContext db) => new(
        new ReservationRepository(db),
        new ResourceRepository(db),
        new UserRepository(db),
        new BlockedDayRepository(db),
        new FakeTimeProvider(Ahora));

    private static string Describe(Exception ex)
    {
        var partes = new List<string>();

        for (var actual = ex; actual is not null; actual = actual.InnerException)
        {
            partes.Add(actual is Npgsql.PostgresException pg
                ? $"PostgresException[{pg.SqlState}] {pg.MessageText}"
                : $"{actual.GetType().Name}: {actual.Message}");
        }

        return string.Join(" << ", partes);
    }

    private async Task<(Guid ResourceId, Guid[] UserIds)> SeedAsync(int users)
    {
        await using var db = fixture.CreateContext();

        var resource = new Resource($"Sala {Guid.NewGuid():N}", ResourceType.MeetingRoom, 8);
        db.Resources.Add(resource);

        var created = new List<Guid>();
        for (var i = 0; i < users; i++)
        {
            var user = new User(
                $"socio-{Guid.NewGuid():N}@camar.test",
                "Socio de prueba",
                "hash",
                MembershipPlan.Flex,
                NifDePrueba(),
                new PhoneNumber("600112233"),
                new PostalCode("28001"),
                Ahora);
            db.Users.Add(user);
            created.Add(user.Id);
        }

        await db.SaveChangesAsync();

        return (resource.Id, [.. created]);
    }

    private static int siguienteNif = 10_000_000;

    /// <summary>
    /// Genera un NIF valido distinto cada vez que se llama.
    ///
    /// El contador es estatico y no un indice del bucle porque todos los tests de la clase
    /// comparten el mismo contenedor de Postgres: si cada uno empezara a numerar de cero,
    /// el segundo chocaria contra el indice unico del documento fiscal.
    /// </summary>
    private static TaxId NifDePrueba()
    {
        var numero = Interlocked.Increment(ref siguienteNif);
        var letra = "TRWAGMYFPDXBNJZSQVHLCKE"[numero % 23];

        return new TaxId($"{numero:D8}{letra}");
    }

    [Fact]
    public async Task CreateAsync_CuandoVariosSociosPidenElMismoHuecoALaVez_SoloEntraUno()
    {
        const int peticiones = 24;
        var (resourceId, userIds) = await SeedAsync(peticiones);
        var hueco = HuecoDisputado();

        // Cada peticion usa su propio DbContext: es lo que pasaria con peticiones HTTP distintas.
        var intentos = userIds.Select(async userId =>
        {
            await using var db = fixture.CreateContext();

            try
            {
                await CreateService(db).CreateAsync(userId, resourceId, hueco);
                return (Entro: true, Error: (Exception?)null);
            }
            catch (ConflictException)
            {
                // La rechazo la comprobacion previa o, si llegaron a la vez, la propia constraint.
                return (Entro: false, Error: null);
            }
            catch (Exception ex)
            {
                return (Entro: false, Error: ex);
            }
        });

        var resultados = await Task.WhenAll(intentos);

        var inesperados = resultados.Where(r => r.Error is not null).Select(r => r.Error!).ToList();
        Assert.True(inesperados.Count == 0,
            "Excepciones no traducidas: " + string.Join(" | ", inesperados.Select(Describe)));

        Assert.Equal(1, resultados.Count(r => r.Entro));

        await using var check = fixture.CreateContext();
        var confirmadas = await check.Reservations
            .CountAsync(r => r.ResourceId == resourceId && r.Status == ReservationStatus.Confirmed);

        Assert.Equal(1, confirmadas);
    }

    [Fact]
    public async Task AddAsync_DosReservasSolapadasALaVez_LaBaseDeDatosFrenaLaSegunda()
    {
        // Sin pasar por el servicio: aqui se comprueba la constraint desnuda.
        var (resourceId, userIds) = await SeedAsync(2);
        var hueco = HuecoDisputado();
        var solapada = new Period(hueco.Start.AddMinutes(30), hueco.End.AddMinutes(30));

        await using (var db = fixture.CreateContext())
        {
            await new ReservationRepository(db)
                .AddAsync(new Reservation(resourceId, userIds[0], hueco, 18m, Ahora));
        }

        await using var otro = fixture.CreateContext();
        var repo = new ReservationRepository(otro);

        await Assert.ThrowsAsync<ConflictException>(() =>
            repo.AddAsync(new Reservation(resourceId, userIds[1], solapada, 18m, Ahora)));
    }

    [Fact]
    public async Task AddAsync_ReservasConsecutivas_LasDosCaben()
    {
        var (resourceId, userIds) = await SeedAsync(2);
        var primera = HuecoDisputado();
        var pegada = new Period(primera.End, primera.End.AddHours(1));

        await using var db = fixture.CreateContext();
        var repo = new ReservationRepository(db);

        await repo.AddAsync(new Reservation(resourceId, userIds[0], primera, 18m, Ahora));
        await repo.AddAsync(new Reservation(resourceId, userIds[1], pegada, 18m, Ahora));

        var confirmadas = await db.Reservations
            .CountAsync(r => r.ResourceId == resourceId && r.Status == ReservationStatus.Confirmed);

        Assert.Equal(2, confirmadas);
    }

    [Fact]
    public async Task AddAsync_UnaReservaCancelada_DejaLibreSuHueco()
    {
        var (resourceId, userIds) = await SeedAsync(2);
        var hueco = HuecoDisputado();

        await using var db = fixture.CreateContext();
        var repo = new ReservationRepository(db);

        var primera = new Reservation(resourceId, userIds[0], hueco, 18m, Ahora);
        await repo.AddAsync(primera);

        primera.Cancel(Ahora.AddHours(1));
        await repo.UpdateAsync(primera);

        // El hueco vuelve a estar disponible porque la constraint solo mira las confirmadas.
        await repo.AddAsync(new Reservation(resourceId, userIds[1], hueco, 18m, Ahora));

        var confirmadas = await db.Reservations
            .CountAsync(r => r.ResourceId == resourceId && r.Status == ReservationStatus.Confirmed);

        Assert.Equal(1, confirmadas);
    }
}
