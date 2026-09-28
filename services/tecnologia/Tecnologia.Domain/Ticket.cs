namespace Tecnologia.Domain;
public class Ticket {
 public string Id { get; set; } = "TEC-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
 public string Title { get; set; } = "";
 public string Description { get; set; } = "";
 public string Type { get; set; } = "Bug";
 public string Origin { get; set; } = "Interno";
 public string Priority { get; set; } = "Media";
 public string Requester { get; set; } = "";
 public string Assignee { get; set; } = "Por asignar";
 public string Status { get; set; } = "Nuevo";
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
public class Activity {
 public string Id { get; set; } = Guid.NewGuid().ToString("N");
 public string? TicketId { get; set; }
 public string Text { get; set; } = "";
 public string Visibility { get; set; } = "Nota interna";
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
