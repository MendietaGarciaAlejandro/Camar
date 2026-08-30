using System.Text.Json.Nodes;
using Camar.Api.Contracts;
using Camar.Api.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Camar.Api.OpenApi;

/// <summary>
/// Ejemplos con datos del coworking. Por defecto se generan a partir del tipo
/// ("status": "string", "price": 1), que no ayudan a entender que devuelve cada endpoint
/// ni que aspecto tiene una reserva de verdad.
/// </summary>
public sealed class EjemplosDeCamar : IOpenApiSchemaTransformer
{
    private const string Sala = "019fdd65-91e2-7050-8a19-2688e73a5847";
    private const string Socio = "019fdd65-9418-7422-b2cf-d7e64f104f01";
    private const string Reserva = "019fdd66-6ee2-78bd-8ed2-6c56a2d41fe8";

    private static readonly Dictionary<Type, string> Ejemplos = new()
    {
        [typeof(CreateReservationRequest)] = $$"""
            {
              "resourceId": "{{Sala}}",
              "start": "2026-08-31T10:00:00+00:00",
              "end": "2026-08-31T11:00:00+00:00"
            }
            """,

        [typeof(ReservationResponse)] = $$"""
            {
              "id": "{{Reserva}}",
              "resourceId": "{{Sala}}",
              "userId": "{{Socio}}",
              "start": "2026-08-31T10:00:00+00:00",
              "end": "2026-08-31T11:00:00+00:00",
              "status": "Confirmed",
              "price": 18.00,
              "createdAt": "2026-08-30T17:12:04.118Z",
              "cancelledAt": null,
              "refundAmount": null
            }
            """,

        [typeof(LoginRequest)] = """
            {
              "email": "ana@camar.test",
              "password": "camar-demo-2026"
            }
            """,

        [typeof(AuthResponse)] = $$"""
            {
              "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
              "expiresAt": "2026-08-30T18:12:04.118Z",
              "userId": "{{Socio}}",
              "role": "Member"
            }
            """,

        [typeof(ResourceResponse)] = $$"""
            {
              "id": "{{Sala}}",
              "name": "Sala Orion",
              "type": "MeetingRoom",
              "capacity": 10
            }
            """,

        [typeof(AvailabilityResponse)] = $$"""
            {
              "resourceId": "{{Sala}}",
              "date": "2026-08-31",
              "freeSlots": [
                { "start": "2026-08-31T08:00:00+00:00", "end": "2026-08-31T08:30:00+00:00" },
                { "start": "2026-08-31T08:30:00+00:00", "end": "2026-08-31T09:00:00+00:00" },
                { "start": "2026-08-31T11:00:00+00:00", "end": "2026-08-31T11:30:00+00:00" }
              ]
            }
            """,

        [typeof(BlockDayRequest)] = """
            {
              "date": "2026-12-25",
              "reason": "Navidad"
            }
            """,

        [typeof(CreateResourceRequest)] = """
            {
              "name": "Sala Lyra",
              "type": "MeetingRoom",
              "capacity": 4
            }
            """,
    };

    public Task TransformAsync(
        OpenApiSchema esquema,
        OpenApiSchemaTransformerContext contexto,
        CancellationToken cancelacion)
    {
        if (Ejemplos.TryGetValue(contexto.JsonTypeInfo.Type, out var json))
            esquema.Example = JsonNode.Parse(json);

        return Task.CompletedTask;
    }
}
