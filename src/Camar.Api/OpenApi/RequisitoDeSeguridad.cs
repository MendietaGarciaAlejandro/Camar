using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Camar.Api.OpenApi;

/// <summary>
/// Marca como protegidas solo las operaciones que lo estan de verdad. Se mira la propia
/// autorizacion del endpoint en lugar de declarar la seguridad global, para que el registro
/// y el login sigan apareciendo como abiertos, que es lo que son.
/// </summary>
public sealed class RequisitoDeSeguridad : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operacion,
        OpenApiOperationTransformerContext contexto,
        CancellationToken cancelacion)
    {
        var metadatos = contexto.Description.ActionDescriptor.EndpointMetadata;

        var exigeToken = metadatos.OfType<IAuthorizeData>().Any()
            && !metadatos.OfType<IAllowAnonymous>().Any();

        if (!exigeToken)
            return Task.CompletedTask;

        operacion.Security ??= [];
        operacion.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(EsquemaDeSeguridad.Nombre, contexto.Document)] = [],
        });

        return Task.CompletedTask;
    }
}
