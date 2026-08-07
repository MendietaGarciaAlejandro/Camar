using Camar.Domain.Common;

namespace Camar.Domain.Scheduling;

/// <summary>
/// Dia en el que el coworking no abre aunque el horario semanal diga lo contrario:
/// festivos, obras, cierre por vacaciones.
/// </summary>
public class BlockedDay
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public string Reason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public BlockedDay(DateOnly date, string reason, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        Date = date;
        Reason = Guard.NotBlank(reason).Trim();
        CreatedAt = createdAt;
    }
}
