using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Camar.Api.OpenApi;

/// <summary>
/// Declara el bearer JWT en el documento OpenAPI. Sin esto Scalar no ensena el boton de
/// autorizar y no hay forma de lanzar desde ahi ninguna peticion protegida.
/// </summary>
public sealed class EsquemaDeSeguridad : IOpenApiDocumentTransformer
{
    public const string Nombre = "Bearer";

    public Task TransformAsync(
        OpenApiDocument documento,
        OpenApiDocumentTransformerContext contexto,
        CancellationToken cancelacion)
    {
        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        documento.Components.SecuritySchemes[Nombre] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "El token que devuelve /api/auth/login. Se pega tal cual, sin escribir 'Bearer'.",
        };

        return Task.CompletedTask;
    }
}
