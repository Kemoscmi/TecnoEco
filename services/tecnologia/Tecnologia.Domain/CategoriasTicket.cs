namespace Tecnologia.Domain;

// Catálogo inicial evolutivo, no una taxonomía inmutable ni tipos de trabajo técnico.
public static class CategoriasTicket
{
    public const string Problema = "Problema";
    public const string Consulta = "Consulta";
    public const string SolicitudDeAyuda = "Solicitud de ayuda";
    public const string SugerenciaMejora = "Sugerencia / mejora";
    public static IReadOnlyList<string> Iniciales { get; } =
        Array.AsReadOnly(new[] { Problema, Consulta, SolicitudDeAyuda, SugerenciaMejora });
}
