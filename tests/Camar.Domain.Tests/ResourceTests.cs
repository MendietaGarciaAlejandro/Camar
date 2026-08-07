using Camar.Domain.Resources;

namespace Camar.Domain.Tests;

public class ResourceTests
{

    // Un recurso recién creado está activo → IsActive es true tras construirlo.
    [Fact]
    public void Constructor_CuandoSeCreaElRecurso_EstaActivo()
    {
        var resource = new Resource("Cosito", ResourceType.MeetingRoom, 100);
        Assert.True(resource.IsActive);
    }

    // Desactiva el recurso → IsActive es false tras llamar a Deactivate().
    [Fact]
    public void Deactivate_CuandoSeDesactivaElRecurso_EsInactivo()
    {
        var resource = new Resource("Testeo", ResourceType.MeetingRoom, 100);
        resource.Deactivate();
        Assert.False(resource.IsActive);
    }

    // Desactivate() dos veces lanza InvalidOperationException la segunda vez.
    [Fact]
    public void Deactivate_CuandoSeDesactivaElRecursoDosVeces_LanzaInvalidOperationException()
    {
        var resource = new Resource("Testeooo", ResourceType.MeetingRoom, 100);
        resource.Deactivate();
        Assert.Throws<InvalidOperationException>(() => resource.Deactivate());
    }

    // Guardas del constructor, nombre en blanco y aforo a 0
    [Theory]
    [InlineData("", ResourceType.MeetingRoom, 100)]
    public void Constructor_CuandoSeCreaElRecursoConParametrosInvalidos_LanzaArgumentException(string name, ResourceType type, int capacity)
    {
        Assert.Throws<ArgumentException>(() => new Resource(name, type, capacity));
    }

    // Guaras aforo a 0
    [Theory]
    [InlineData("Nombre", ResourceType.MeetingRoom, 0)]
    public void Constructor_CuandoSeCreaElRecursoConAforoCero_LanzaArgumentException(string name, ResourceType type, int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Resource(name, type, capacity));
    }
}
