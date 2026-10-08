namespace Tecnologia.Domain;
public class Ticket {
 public string Id { get; set; } = "TEC-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
 public string Title { get; set; } = "";
 public string Description { get; set; } = "";
 public string Categoria { get; set; } = CategoriasTicket.Problema;
 public required string SolicitanteId { get; set; }
 public string? ResponsableId { get; set; }
 public string Estado { get; set; } = EstadosTicket.Recibida;
 public List<TicketColaborador> Colaboradores { get; set; } = [];
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
public class Activity {
 public string Id { get; set; } = Guid.NewGuid().ToString("N");
 public string? TicketId { get; set; }
 public string Text { get; set; } = "";
 public string Visibility { get; set; } = "Nota interna";
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
