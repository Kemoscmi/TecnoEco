namespace Tecnologia.Domain;

// Referencia opaca a identidad externa; no almacena datos personales ni perfiles.
public class TicketColaborador
{
    public required string TicketId { get; set; }
    public required string ColaboradorId { get; set; }
}
