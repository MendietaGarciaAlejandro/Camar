namespace Camar.Domain.Resources;

/// <summary>
/// Como se llama cada tipo de recurso cuando el texto lo va a leer un socio.
///
/// Los nombres del enum son de codigo y estan en ingles como el resto del proyecto, asi que
/// ToString() no sirve para un mensaje: "Una reserva de HotDesk dura..." queda a medio
/// traducir. Los nombres tecnicos siguen saliendo tal cual en el JSON de la API, que ahi es
/// justo lo que quiere quien la consume.
/// </summary>
public static class ResourceTypeNames
{
    public static string DisplayName(this ResourceType type) => type switch
    {
        ResourceType.MeetingRoom => "sala de reuniones",
        ResourceType.HotDesk => "mesa flexible",
        ResourceType.PhoneBooth => "cabina de llamadas",
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };
}
