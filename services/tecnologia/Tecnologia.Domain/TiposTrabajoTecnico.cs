namespace Tecnologia.Domain;

// Vocabulario conceptual para referencias. No modela módulos de trabajo técnico.
public static class TiposTrabajoTecnico
{
    public const string Bug = "Bug";
    public const string HistoriaDeUsuario = "Historia de usuario";
    public const string TareaTecnica = "Tarea técnica";
    public const string Mejora = "Mejora";
    public static IReadOnlyList<string> Todos { get; } =
        Array.AsReadOnly(new[] { Bug, HistoriaDeUsuario, TareaTecnica, Mejora });

    public static string Validar(string? tipo)
    {
        if (tipo is null || !Todos.Contains(tipo)) throw new ArgumentException("Tipo de trabajo técnico no válido.");
        return tipo;
    }
}
