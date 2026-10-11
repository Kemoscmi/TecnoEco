namespace Tecnologia.Domain;
public class Ticket {
 public string Id { get; set; } = "TEC-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
 public string Title { get; set; } = "";
 public string Description { get; set; } = "";
 public string Categoria { get; set; } = CategoriasTicket.Problema;
 public required string SolicitanteId { get; set; }
 public string? ResponsableId { get; set; }
 public string Estado { get; set; } = EstadosTicket.Recibida;
 public DateTimeOffset? AbandonoRecibidaAt { get; set; }
 public DateTimeOffset? EliminadoAt { get; set; }
 public string? EliminadoPorId { get; set; }
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
// REQ-010: metadatos de un archivo adjunto.
// Url es null hasta que se configure el proveedor de storage.
// Visibility se hereda del elemento padre (ticket original = "publica"; actividad = según su Visibility).
public class Adjunto {
 public string Id { get; set; } = Guid.NewGuid().ToString("N");
 public string TicketId { get; set; } = "";
 public string? ActivityId { get; set; }
 public string Nombre { get; set; } = "";
 public string Tipo { get; set; } = "";
 public long Tamaño { get; set; }
 public string Visibility { get; set; } = "publica"; // publica | interna
 public string? Url { get; set; }
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
