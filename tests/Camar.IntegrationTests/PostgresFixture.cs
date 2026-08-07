using Camar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Camar.IntegrationTests;

/// <summary>
/// Levanta un Postgres real en un contenedor y le aplica las migraciones.
/// Hace falta uno de verdad: la constraint de exclusion gist no existe en un proveedor en memoria.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .WithDatabase("camar_tests")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public CamarDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CamarDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
