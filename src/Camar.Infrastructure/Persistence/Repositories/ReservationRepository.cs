using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository(CamarDbContext db) : IReservationRepository
{
    // 23P01 = exclusion_violation. Lo lanza la constraint ck_reservations_no_overlap.
    private const string ExclusionViolation = "23P01";

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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: ExclusionViolation })
        {
            // Dos peticiones simultaneas pasaron la comprobacion previa y la BD freno a esta.
            // Se traduce aqui para que Application no tenga que conocer Npgsql.
            throw new ConflictException("Ese hueco acaba de ser reservado por otra persona.");
        }
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
