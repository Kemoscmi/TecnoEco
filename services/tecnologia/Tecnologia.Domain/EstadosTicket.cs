namespace Tecnologia.Domain;

// REQ-004 define el vocabulario y valida los cambios explícitos.
// No existe una matriz de transiciones: las reglas adicionales siguen pendientes.
public static class EstadosTicket
{
    public const string Recibida = "Recibida";
    public const string EnProceso = "En proceso";
    public const string NecesitamosInformacion = "Necesitamos información";
    public const string Resuelta = "Resuelta";
    public const string Rechazada = "Rechazada";
    public static IReadOnlyList<string> Todos { get; } =
        Array.AsReadOnly(new[] { Recibida, EnProceso, NecesitamosInformacion, Resuelta, Rechazada });

    public static string Validar(string? estado)
    {
        if (estado is null || !Todos.Contains(estado))
            throw new ArgumentException("Estado no válido.");

        return estado;
    }
}
