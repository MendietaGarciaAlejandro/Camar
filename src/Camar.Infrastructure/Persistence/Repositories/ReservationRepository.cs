using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository(CamarDbContext db) : IReservationRepository
{
    // 23P01 = exclusion_violation: la constraint ck_reservations_no_overlap rechazo el solapamiento.
    private const string ExclusionViolation = "23P01";

    // 40P01 = deadlock_detected. Con varias reservas simultaneas sobre el mismo hueco, cada
    // transaccion espera a ver si la anterior confirma; si esas esperas se cruzan, Postgres
    // mata a una. Para quien reserva significa lo mismo: alguien se le adelanto.
    private const string DeadlockDetected = "40P01";

    // El periodo se guarda como tstzrange, asi que EF no puede traducir r.Period.Start.
    // Se consulta con el operador && de Postgres, que es justo lo que usa la constraint.
    public Task<bool> HasOverlapAsync(Guid resourceId, Period period, CancellationToken ct = default) =>
        db.Reservations
            .FromSql($"""
                SELECT * FROM reservations
                WHERE resource_id = {resourceId}
                  AND status = {(int)ReservationStatus.Confirmed}
                  AND period && tstzrange({period.Start}, {period.End}, '[)')
                """)
            .AnyAsync(ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        db.Reservations.Add(reservation);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (IsSlotConflict(ex))
        {
            // Dos peticiones simultaneas pasaron la comprobacion previa y la BD freno a esta.
            // Se traduce aqui para que Application no tenga que conocer Npgsql.
            db.Entry(reservation).State = EntityState.Detached;

            throw new ConflictException("Ese hueco acaba de ser reservado por otra persona.");
        }
    }

    /// <summary>
    /// EF envuelve los fallos de Postgres en varias capas, asi que se recorre la cadena entera.
    /// </summary>
    private static bool IsSlotConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: ExclusionViolation or DeadlockDetected })
                return true;
        }

        return false;
    }

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(
        Guid? resourceId = null, CancellationToken ct = default)
    {
        var query = db.Reservations.AsQueryable();

        if (resourceId is { } id)
            query = query.Where(r => r.ResourceId == id);

        var found = await query.ToListAsync(ct);

        // Igual que en GetByUserAsync: el inicio del rango no se traduce a SQL.
        return found.OrderByDescending(r => r.Period.Start).ToList();
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken ct = default)
    {
        db.Reservations.Update(reservation);
        return db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var found = await db.Reservations
            .Where(r => r.UserId == userId)
            .ToListAsync(ct);

        // Ordenar por el inicio del rango tampoco se traduce; con las reservas
        // de un solo usuario ordenar en memoria sale barato.
        return found.OrderByDescending(r => r.Period.Start).ToList();
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Reservation>> GetConfirmedInRangeAsync(
        Guid resourceId, Period range, CancellationToken ct = default) =>
        await db.Reservations
            .FromSql($"""
                SELECT * FROM reservations
                WHERE resource_id = {resourceId}
                  AND status = {(int)ReservationStatus.Confirmed}
                  AND period && tstzrange({range.Start}, {range.End}, '[)')
                """)
            .ToListAsync(ct);
}
