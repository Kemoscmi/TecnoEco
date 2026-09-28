using Tecnologia.Application;
using Tecnologia.Domain;

var repo = new MemoryRepository();
var service = new TechnologyService(repo);
var ct = CancellationToken.None;
var input = new TicketInput(" Error de guardado ", "Pasos para reproducir", "Bug", "Cliente", "Alta", "Cliente de ejemplo", "Desarrollo");
var data = await service.Create(input, ct);
Check(data.Tickets.Count == 1 && data.Activities.Count == 1, "Crear ticket registra actividad");
var ticket = data.Tickets.Single();
Check(ticket.Title == "Error de guardado" && ticket.Status == "Nuevo", "Normalización y estado inicial");
await service.ChangeStatus(ticket.Id, new StatusInput("En proceso"), ct);
Check(ticket.Status == "En proceso" && repo.Log.Count == 2, "Cambio de estado con bitácora");
await service.ChangeStatus(ticket.Id, new StatusInput("En proceso"), ct);
Check(repo.Log.Count == 2, "No duplicar cambios sin efecto");
await Reject<ArgumentException>(() => service.ChangeStatus(ticket.Id, new StatusInput("Inventado"), ct), "Rechazar estado inválido");
await Reject<ArgumentException>(() => service.Create(input with { Title = " " }, ct), "Rechazar título vacío");
await Reject<KeyNotFoundException>(() => service.AddActivity(new ActivityInput("inexistente", "Nota", "Nota interna"), ct), "Rechazar ticket inexistente");
await Reject<ArgumentException>(() => service.AddActivity(new ActivityInput(null, "Nota", "Respuesta al solicitante"), ct), "Actividad general interna");
await service.AddActivity(new ActivityInput(null, "Mantenimiento", "Nota interna"), ct);
await service.AddActivity(new ActivityInput(ticket.Id, "Solución registrada", "Respuesta al solicitante"), ct);
Check(repo.Log.Count == 4 && repo.Log.Last().Visibility == "Respuesta al solicitante", "Actividades generales y respuestas");
Console.WriteLine("9 comprobaciones correctas.");

static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("OK: " + message); }
static async Task Reject<T>(Func<Task<Snapshot>> action, string message) where T : Exception {
    try { await action(); } catch (T) { Console.WriteLine("OK: " + message); return; }
    throw new Exception(message);
}
sealed class MemoryRepository : ITechnologyRepository {
    public List<Ticket> Items { get; } = [];
    public List<Activity> Log { get; } = [];
    public Task<List<Ticket>> Tickets(CancellationToken ct) => Task.FromResult(Items.ToList());
    public Task<List<Activity>> Activities(CancellationToken ct) => Task.FromResult(Log.ToList());
    public Task<Ticket?> Find(string id, CancellationToken ct) => Task.FromResult(Items.FirstOrDefault(t => t.Id == id));
    public void Add(Ticket ticket) => Items.Add(ticket);
    public void Add(Activity activity) => Log.Add(activity);
    public Task Save(CancellationToken ct) => Task.CompletedTask;
}
