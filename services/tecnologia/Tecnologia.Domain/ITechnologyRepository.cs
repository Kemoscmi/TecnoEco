namespace Tecnologia.Domain;
public interface ITechnologyRepository {
 Task<List<Ticket>> Tickets(CancellationToken ct);
 Task<List<Activity>> Activities(CancellationToken ct);
 Task<List<Adjunto>> Adjuntos(CancellationToken ct);
 Task<Ticket?> Find(string id, CancellationToken ct);
 void Add(Ticket ticket);
 void Add(Activity activity);
 void Add(Adjunto adjunto);
 void Add(TrabajoTecnicoRelacionado trabajoTecnico);
 void Add(Notificacion notificacion);
 Task<List<Notificacion>> Notificaciones(string destinatarioId, CancellationToken ct);
 Task<Notificacion?> FindNotificacion(string id, CancellationToken ct);
 Task Save(CancellationToken ct);
}
