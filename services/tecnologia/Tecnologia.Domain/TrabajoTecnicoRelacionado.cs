namespace Tecnologia.Domain;

// Referencia mínima a trabajo técnico externo o futuro; no crea ni administra ese trabajo.
public class TrabajoTecnicoRelacionado
{
    public required string TicketId { get; set; }
    public required string TrabajoTecnicoId { get; set; }
    public string TipoTrabajoTecnico { get; set; } = TiposTrabajoTecnico.TareaTecnica;
    public required string RelacionadoPorId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
