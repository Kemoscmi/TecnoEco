using Microsoft.EntityFrameworkCore;
using Tecnologia.Domain;
namespace Tecnologia.Infrastructure;
public class TechnologyRepository(TechnologyDbContext db):ITechnologyRepository {
 public Task<List<Ticket>> Tickets(CancellationToken ct)=>db.Tickets.Include(x=>x.Colaboradores).AsNoTracking().OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
 public Task<List<Activity>> Activities(CancellationToken ct)=>db.Activities.AsNoTracking().OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
 public Task<Ticket?> Find(string id,CancellationToken ct)=>db.Tickets.Include(x=>x.Colaboradores).FirstOrDefaultAsync(x=>x.Id==id,ct);
 public void Add(Ticket ticket)=>db.Tickets.Add(ticket);
 public void Add(Activity activity)=>db.Activities.Add(activity);
 public async Task Save(CancellationToken ct)=>await db.SaveChangesAsync(ct);
}
