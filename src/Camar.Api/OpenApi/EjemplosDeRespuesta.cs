using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Camar.Api.OpenApi;

/// <summary>
/// Ejemplos atados a una operacion concreta. Crear y cancelar devuelven el mismo tipo, asi
/// que con un solo ejemplo por esquema la cancelacion salia como confirmada y sin reembolso,
/// que es justo lo contrario de lo que hace.
/// </summary>
public sealed class EjemplosDeRespuesta : IOpenApiOperationTransformer
{
    private const string Cancelada = """
        {
          "id": "019fdd66-6ee2-78bd-8ed2-6c56a2d41fe8",
          "resourceId": "019fdd65-91e2-7050-8a19-2688e73a5847",
          "userId": "019fdd65-9418-7422-b2cf-d7e64f104f01",
          "start": "2026-08-31T10:00:00+00:00",
          "end": "2026-08-31T11:00:00+00:00",
          "status": "Cancelled",
          "price": 18.00,
          "createdAt": "2026-08-30T17:12:04.118Z",
          "cancelledAt": "2026-08-30T18:40:11.902Z",
          "refundAmount": 18.00
        }
        """;

    public Task TransformAsync(
        OpenApiOperation operacion,
        OpenApiOperationTransformerContext contexto,
        CancellationToken cancelacion)
    {
        var ruta = contexto.Description.RelativePath;

        if (ruta is "api/reservations/{id}/cancel")
            PonerEjemplo(operacion, "200", Cancelada);

        return Task.CompletedTask;
    }

    private static void PonerEjemplo(OpenApiOperation operacion, string codigo, string json)
    {
        if (operacion.Responses?.TryGetValue(codigo, out var respuesta) is not true)
            return;

        if (respuesta.Content?.TryGetValue("application/json", out var medio) is true)
            medio.Example = JsonNode.Parse(json);
    }
}
