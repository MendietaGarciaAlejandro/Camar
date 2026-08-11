using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Members;
using Camar.Domain.Pricing;
using Camar.Domain.Reservations;
using Camar.Domain.Resources;
using Camar.Domain.Scheduling;

namespace Camar.Application.Reservations;

public class ReservationService(
    IReservationRepository reservations,
    IResourceRepository resources,
    IUserRepository users,
    IBlockedDayRepository blockedDays,
    TimeProvider clock)
{
    public async Task<Reservation> CreateAsync(
        Guid userId,
        Guid resourceId,
        Period period,
        CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"No existe el usuario {userId}.");

        var resource = await resources.GetByIdAsync(resourceId, ct)
            ?? throw new NotFoundException($"No existe el recurso {resourceId}.");

        if (!resource.IsActive)
            throw new BusinessRuleException($"El recurso '{resource.Name}' no esta disponible.");

        if (!BookingRules.IsAligned(period))
            throw new BusinessRuleException("Las reservas empiezan y terminan en punto o y media.");

        if (!BookingRules.IsValidDuration(resource.Type, period))
        {
            var (min, max) = BookingRules.DurationLimits(resource.Type);
            throw new BusinessRuleException(
                $"Una reserva de {resource.Type.DisplayName()} dura {DurationText.DescribeRange(min, max)}.");
        }

        if (!OpeningHoursPolicy.IsWithinOpeningHours(period))
            throw new BusinessRuleException("La reserva queda fuera del horario de apertura.");

        var today = DateOnly.FromDateTime(clock.GetUtcNow().Date);
        var reservationDate = DateOnly.FromDateTime(period.Start.Date);

        if (await blockedDays.GetByDateAsync(reservationDate, ct) is { } blocked)
            throw new BusinessRuleException($"El {blocked.Date:dd/MM/yyyy} el coworking no abre: {blocked.Reason}.");

        if (!MembershipPolicy.CanBookOn(user.MembershipPlan, today, reservationDate))
        {
            var dias = MembershipPolicy.MaxAdvanceDays(user.MembershipPlan);
            throw new BusinessRuleException(
                $"El plan {user.MembershipPlan.DisplayName()} reserva como mucho con " +
                (dias == 1 ? "un dia de antelacion." : $"{dias} dias de antelacion."));
        }

        // Comprobacion amable: da un error claro en vez de dejar que reviente la constraint.
        // La garantia real la pone la BD, que rechaza el insert si dos peticiones llegan a la vez.
        if (await reservations.HasOverlapAsync(resourceId, period, ct))
            throw new ConflictException("Ese hueco ya esta reservado.");

        var price = PricingPolicy.CalculatePrice(resource.Type, period);

        var reservation = new Reservation(resourceId, userId, period, price, clock.GetUtcNow());
        await reservations.AddAsync(reservation, ct);

        return reservation;
    }

    /// <summary>
    /// Cancela una reserva del usuario. El reembolso lo decide la politica de cancelacion.
    /// </summary>
    public async Task<Reservation> CancelAsync(
        Guid reservationId,
        Guid userId,
        CancellationToken ct = default)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, ct);

        // Si la reserva es de otro, se responde igual que si no existiera:
        // asi no se puede averiguar que ids son validos probando.
        if (reservation is null || reservation.UserId != userId)
            throw new NotFoundException($"No existe la reserva {reservationId}.");

        try
        {
            reservation.Cancel(clock.GetUtcNow());
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await reservations.UpdateAsync(reservation, ct);

        return reservation;
    }
}
