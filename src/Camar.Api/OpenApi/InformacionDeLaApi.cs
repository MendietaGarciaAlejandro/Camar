using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Camar.Api.OpenApi;

/// <summary>
/// Portada del documento. Por defecto el titulo sale del nombre del ensamblado
/// ("Camar.Api"), que no dice gran cosa a quien abre la referencia.
/// </summary>
public sealed class InformacionDeLaApi : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument documento,
        OpenApiDocumentTransformerContext contexto,
        CancellationToken cancelacion)
    {
        documento.Info.Title = "Camar";
        documento.Info.Version = "v1";
        documento.Info.Description =
            "API de reservas de Camar Coworking. Salas de reunion, mesas flexibles y cabina "
            + "de llamadas, cada recurso con sus propias reglas de horario y duracion.\n\n"
            + "Casi todo exige token: primero **/api/auth/login** y el token que devuelve se "
            + "pega en Authentication, arriba a la derecha. En desarrollo el sembrador deja "
            + "creados `ana@camar.test` (socio) y `admin@camar.test` (administracion), los dos "
            + "con la contrasena `camar-demo-2026`.";

        return Task.CompletedTask;
    }
}
