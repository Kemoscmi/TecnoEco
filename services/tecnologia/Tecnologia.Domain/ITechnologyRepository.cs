namespace Tecnologia.Domain;
public interface ITechnologyRepository {
 Task<List<Ticket>> Tickets(CancellationToken ct);
 Task<List<Activity>> Activities(CancellationToken ct);
 Task<Ticket?> Find(string id, CancellationToken ct);
 void Add(Ticket ticket);
 void Add(Activity activity);
 Task Save(CancellationToken ct);
}
