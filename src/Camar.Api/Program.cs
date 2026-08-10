using System.Text;
using Camar.Api.ErrorHandling;
using Camar.Application.Abstractions;
using Camar.Application.Auth;
using Camar.Application.Reservations;
using Camar.Infrastructure;
using Camar.Infrastructure.Persistence;
using Camar.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Camar")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexion 'Camar'. En desarrollo se configura con user-secrets.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la seccion 'Jwt' de configuracion.");

if (string.IsNullOrWhiteSpace(jwt.SigningKey))
    throw new InvalidOperationException(
        "Falta 'Jwt:SigningKey'. En desarrollo se configura con user-secrets.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            // Sin margen extra: un token caducado deja de valer al segundo.
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// El cliente web de Estanza corre en otro origen, asi que el navegador exige CORS. Los
// origenes se leen de configuracion en vez de fijarlos aqui: en desarrollo es localhost
// con el puerto que asigne el servidor de Compose, y en produccion sera otra cosa.
const string PoliticaCors = "clientes";
var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos")
    .Get<string[]>() ?? [];

builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy(PoliticaCors, politica =>
    {
        politica.WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
        app.Logger);
}

// En desarrollo no se fuerza HTTPS: el cliente movil apunta a la IP de la red local por
// HTTP, porque el certificado de desarrollo no lo acepta un dispositivo Android.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(PoliticaCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
