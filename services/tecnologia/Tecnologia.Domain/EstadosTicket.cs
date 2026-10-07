namespace Tecnologia.Domain;

// Solo vocabulario; REQ-004 definirá las reglas de transición.
public static class EstadosTicket
{
    public const string Recibida = "Recibida";
    public const string EnProceso = "En proceso";
    public const string NecesitamosInformacion = "Necesitamos información";
    public const string Resuelta = "Resuelta";
    public const string Rechazada = "Rechazada";
    public static IReadOnlyList<string> Todos { get; } =
        Array.AsReadOnly(new[] { Recibida, EnProceso, NecesitamosInformacion, Resuelta, Rechazada });
}
