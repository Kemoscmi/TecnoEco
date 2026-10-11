namespace Tecnologia.Domain;
// REQ-015: registro de una notificación generada por un evento relevante de la solicitud.
// Canal (interno/correo) se resuelve cuando esté definida la infraestructura de transporte.
public class Notificacion {
 public string Id { get; set; } = Guid.NewGuid().ToString("N");
 public string DestinatarioId { get; set; } = "";
 public string TicketId { get; set; } = "";
 public string TipoNotificacion { get; set; } = "";
 public string Mensaje { get; set; } = "";
 public bool Leida { get; set; } = false;
 public DateTimeOffset FechaUtc { get; set; } = DateTimeOffset.UtcNow;
}
