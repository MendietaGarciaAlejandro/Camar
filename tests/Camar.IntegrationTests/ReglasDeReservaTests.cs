using Camar.Application.Reservations;
using Camar.Domain.Common;
using Camar.Domain.Members;
using Camar.Domain.Reservations;
using Camar.Domain.Resources;
using Camar.Infrastructure.Persistence;
using Camar.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Time.Testing;

namespace Camar.IntegrationTests;

/// <summary>
/// Las reglas que rechazan una reserva, y sobre todo con que mensaje.
///
/// El mensaje importa tanto como el rechazo: es lo unico que ve el socio, y el cliente lo
/// enseña tal cual en vez de reescribirlo. Si aqui se dice una cosa por otra, el usuario se
/// queda mirando la pantalla sin entender que ha hecho mal.
/// </summary>
public class ReglasDeReservaTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    // Lunes 12/01/2026.
    private static readonly DateTimeOffset Ahora = new(2026, 1, 12, 8, 0, 0, TimeSpan.Zero);

    private static int siguienteNif = 50_000_000;

    /// <summary>
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

    /// <param name="momento">
    /// Cuando se supone que estamos. Por defecto, las ocho de la mañana del lunes 12.
    /// </param>
    private ReservationService CreateService(CamarDbContext db, DateTimeOffset? momento = null) => new(
        new ReservationRepository(db),
        new ResourceRepository(db),
        new UserRepository(db),
        new BlockedDayRepository(db),
        new FakeTimeProvider(momento ?? Ahora));

    private async Task<(Guid ResourceId, Guid UserId)> SeedAsync(
        MembershipPlan plan = MembershipPlan.Flex,
        ResourceType tipo = ResourceType.MeetingRoom)
    {
        await using var db = fixture.CreateContext();

        var resource = new Resource($"Sala {Guid.NewGuid():N}", tipo, 8);
        db.Resources.Add(resource);

        var user = new User(
            $"socio-{Guid.NewGuid():N}@camar.test",
            "Socio de prueba",
            "hash",
            plan,
            NifDePrueba(),
            new PhoneNumber("600112233"),
            new PostalCode("28001"),
            Ahora);
        db.Users.Add(user);

        await db.SaveChangesAsync();

        return (resource.Id, user.Id);
    }

    private static Period ElDia(int dia, int horaInicio = 10, int horaFin = 11) => new(
        new DateTimeOffset(2026, 1, dia, horaInicio, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, dia, horaFin, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task UnDiaQueYaPaso_LoDiceTalCual()
    {
        var (resourceId, userId) = await SeedAsync();
        await using var db = fixture.CreateContext();

        // El 9 es viernes, dentro del horario, pero anterior al "hoy" del reloj falso.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(db).CreateAsync(userId, resourceId, ElDia(9)));

        // Antes esto contestaba "el plan Flex reserva como mucho con 7 dias de antelacion",
        // que no tiene nada que ver con lo que el socio acaba de intentar.
        Assert.Equal("No se puede reservar un dia que ya ha pasado.", error.Message);
    }

    [Fact]
    public async Task UnaHoraDeHoyQueYaPaso_TampocoSeReserva()
    {
        var (resourceId, userId) = await SeedAsync();
        await using var db = fixture.CreateContext();

        // Son las tres de la tarde y se intenta reservar de nueve a diez de esta mañana.
        // El dia es correcto, asi que la comprobacion por fecha no lo cazaba.
        var mediaTarde = new DateTimeOffset(2026, 1, 12, 15, 0, 0, TimeSpan.Zero);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(db, mediaTarde).CreateAsync(userId, resourceId, ElDia(12, 9, 10)));

        Assert.Equal("No se puede reservar una hora que ya ha pasado.", error.Message);
    }

    [Fact]
    public async Task LoQueQuedaDeHoy_SeSigueReservando()
    {
        // El contrapeso del test de arriba: la regla no puede llevarse por delante una
        // reserva perfectamente valida para dentro de un rato.
        var (resourceId, userId) = await SeedAsync();
        await using var db = fixture.CreateContext();

        var mediaTarde = new DateTimeOffset(2026, 1, 12, 15, 0, 0, TimeSpan.Zero);

        var reserva = await CreateService(db, mediaTarde)
            .CreateAsync(userId, resourceId, ElDia(12, 17, 18));

        Assert.Equal(ReservationStatus.Confirmed, reserva.Status);
    }

    [Fact]
    public async Task DemasiadaAntelacion_HablaDelPlan()
    {
        // El Bono de dia solo llega a mañana; el 20 se le va de largo.
        var (resourceId, userId) = await SeedAsync(MembershipPlan.DayPass);
        await using var db = fixture.CreateContext();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(db).CreateAsync(userId, resourceId, ElDia(20)));

        Assert.Equal(
            "El plan Bono de dia reserva como mucho con un dia de antelacion.",
            error.Message);
    }

    [Fact]
    public async Task ElPlanFlexLlegaASieteDias_YLoDiceEnPlural()
    {
        var (resourceId, userId) = await SeedAsync();
        await using var db = fixture.CreateContext();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(db).CreateAsync(userId, resourceId, ElDia(21)));

        Assert.Equal(
            "El plan Flex reserva como mucho con 7 dias de antelacion.",
            error.Message);
    }

    [Fact]
    public async Task UnaDuracionQueNoCuadra_SeDiceEnHoras()
    {
        // Una hora en una mesa flexible, que tiene un minimo de cuatro.
        var (resourceId, userId) = await SeedAsync(tipo: ResourceType.HotDesk);
        await using var db = fixture.CreateContext();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(db).CreateAsync(userId, resourceId, ElDia(14, 10, 11)));

        Assert.Equal(
            "Una reserva de mesa flexible dura entre 4 y 13 horas.",
            error.Message);
    }
}
