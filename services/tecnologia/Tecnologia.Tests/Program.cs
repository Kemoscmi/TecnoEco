using Tecnologia.Application;
using Tecnologia.Domain;
using System.Text.Json;

var repo = new MemoryRepository();
var service = new TechnologyService(repo);
var ct = CancellationToken.None;
var input = new TicketInput(" Error de guardado ", "Pasos para reproducir", CategoriasTicket.Problema, "Cliente-01");
var data = await service.Create(input, ct);
Check(data.Tickets.Count == 1 && data.Activities.Count == 1, "Crear ticket registra actividad");
var ticket = data.Tickets.Single();
Check(ticket.Title == "Error de guardado" && ticket.Estado == EstadosTicket.Recibida, "Normalización y estado inicial");
await service.Load(ct);
Check(ticket.Estado == EstadosTicket.Recibida && repo.Log.Count == 1, "Consultar no cambia el estado");
await service.ChangeStatus(ticket.Id, new StatusInput(EstadosTicket.NecesitamosInformacion), ct);
Check(ticket.Estado == EstadosTicket.NecesitamosInformacion && repo.Log.Count == 2, "Tecnología solicita información mediante cambio explícito");
await service.AddActivity(new ActivityInput(ticket.Id, "El solicitante proporcionó los datos solicitados", "Respuesta al solicitante"), ct);
Check(ticket.Estado == EstadosTicket.NecesitamosInformacion, "Respuesta del solicitante no cambia el estado");
await service.ChangeStatus(ticket.Id, new StatusInput(EstadosTicket.EnProceso), ct);
Check(ticket.Estado == EstadosTicket.EnProceso && repo.Log.Count == 4, "Tecnología devuelve manualmente a En proceso");
await service.ChangeStatus(ticket.Id, new StatusInput("En proceso"), ct);
Check(repo.Log.Count == 4, "No duplicar cambios sin efecto");
await Reject<ArgumentException>(() => service.ChangeStatus(ticket.Id, new StatusInput("Inventado"), ct), "Rechazar estado inválido");
await Reject<ArgumentException>(() => service.Create(input with { Title = " " }, ct), "Rechazar título vacío");
await Reject<KeyNotFoundException>(() => service.AddActivity(new ActivityInput("inexistente", "Nota", "Nota interna"), ct), "Rechazar ticket inexistente");
await Reject<ArgumentException>(() => service.AddActivity(new ActivityInput(null, "Nota", "Respuesta al solicitante"), ct), "Actividad general interna");
await service.AddActivity(new ActivityInput(null, "Mantenimiento", "Nota interna"), ct);
await service.AddActivity(new ActivityInput(ticket.Id, "Solución registrada", "Respuesta al solicitante"), ct);
Check(ticket.Estado == EstadosTicket.EnProceso && repo.Log.Count == 6 && repo.Log.Last().Visibility == "Respuesta al solicitante", "Actividades y trabajo técnico no cambian el estado automáticamente");
Check(ticket.ResponsableId is null && ticket.Colaboradores.Count == 0, "Responsable opcional y cero colaboradores");
Check(ticket.CreatedAt.Offset == TimeSpan.Zero && ticket.Id.StartsWith("TEC-"), "Identidad del Ticket y fecha UTC");
foreach (var categoria in CategoriasTicket.Iniciales)
{
    var result = await service.Create(input with { Categoria = categoria, SolicitanteId = "Identity:Aa-01" }, ct);
    var created = result.Tickets.Last();
    Check(created.Categoria == categoria && created.SolicitanteId == "Identity:Aa-01" && created.ResponsableId is null, "Categoría e identificadores opacos: " + categoria);
}
await Reject<ArgumentException>(() => service.Create(input with { Categoria = "Bug" }, ct), "Ticket no es Bug");
await Reject<ArgumentException>(() => service.Create(input with { SolicitanteId = " " }, ct), "Solicitante obligatorio");
await Reject<ArgumentException>(() => service.Create(input with { SolicitanteId = new string('x', 161) }, ct), "Límite de identificador");
await Reject<ArgumentException>(() => service.Create(input with { Description = " " }, ct), "Descripción obligatoria");
Check(typeof(TicketInput).GetProperty(nameof(Ticket.ResponsableId)) is null, "Contrato de creación no admite responsable");
var inputApi = JsonSerializer.Deserialize<TicketInput>("""{"title":"Solicitud API","description":"Prueba del contrato","categoria":"Consulta","solicitanteId":"cliente-api","responsableId":"soporte-01"}""", new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
var ticketApi = (await service.Create(inputApi, ct)).Tickets.Last();
Check(ticketApi.ResponsableId is null, "Cuerpo de creación no asigna responsable");
ticket.ResponsableId = "soporte-01";
Check(ticket.ResponsableId == "soporte-01", "Modelo permite asignación posterior");
Check(EstadosTicket.Todos.SequenceEqual(new[] { "Recibida", "En proceso", "Necesitamos información", "Resuelta", "Rechazada" }), "Únicamente los cinco estados acordados");
var evento = new EventoTicket(ticket.Id, "actor-01", new DateTimeOffset(2026, 10, 7, 9, 0, 0, TimeSpan.FromHours(-6)), "ejemplo-contrato");
Check(evento.FechaUtc.Offset == TimeSpan.Zero && evento.FechaUtc.Hour == 15 && evento.ActorId == "actor-01" && evento.TicketId == ticket.Id && evento.TipoEvento == "ejemplo-contrato", "Contrato de evento normaliza UTC sin generar auditoría");
await PersistenceChecks.Run();
Console.WriteLine("Todas las comprobaciones REQ-001 correctas.");

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
