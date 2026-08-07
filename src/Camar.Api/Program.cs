using Camar.Api.ErrorHandling;
using Camar.Application.Reservations;
using Camar.Infrastructure;
using Camar.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Camar")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexion 'Camar'. En desarrollo se configura con user-secrets.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<ReservationService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    await DevelopmentSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<CamarDbContext>(),
        scope.ServiceProvider.GetRequiredService<TimeProvider>(),
        app.Logger);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
