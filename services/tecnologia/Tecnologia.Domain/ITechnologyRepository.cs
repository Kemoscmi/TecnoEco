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
 Task Save(CancellationToken ct);
}
