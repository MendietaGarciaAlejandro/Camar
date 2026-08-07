using Camar.Domain.Members;
using Camar.Domain.Reservations;
using Camar.Domain.Resources;
using Camar.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Camar.Infrastructure.Persistence;

public class CamarDbContext : DbContext
{
    public CamarDbContext(DbContextOptions<CamarDbContext> options)
        : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BlockedDay> BlockedDays => Set<BlockedDay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // btree_gist hace falta para la constraint de exclusion de la siguiente migracion
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CamarDbContext).Assembly);
    }
}