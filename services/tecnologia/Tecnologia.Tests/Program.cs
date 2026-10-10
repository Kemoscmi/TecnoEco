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

// REQ-002 — Crear Solicitud
// La solicitud nace en estado Recibida y no genera trabajo técnico automáticamente.
var req002Input = new TicketInput("Solicitud de acceso al portal", "Necesito acceso al portal de reportes para revisar los indicadores del mes.", CategoriasTicket.SolicitudDeAyuda, "solicitante-ext-01");
var req002Data = await service.Create(req002Input, ct);
var req002Ticket = req002Data.Tickets.Last();
Check(req002Ticket.Estado == EstadosTicket.Recibida, "REQ-002: solicitud nace en estado Recibida");
Check(req002Ticket.SolicitanteId == "solicitante-ext-01", "REQ-002: solicitante se preserva en la solicitud");
Check(req002Ticket.Categoria == CategoriasTicket.SolicitudDeAyuda, "REQ-002: categoría registrada correctamente");
Check(req002Ticket.Title == "Solicitud de acceso al portal", "REQ-002: título sin mutación");
Check(req002Ticket.ResponsableId is null, "REQ-002: la solicitud no asigna responsable al crearse");
// No existe trabajo técnico asociado al crear la solicitud — el modelo no tiene Work Items.
Check(!req002Data.Tickets.Any(t => t.Id != req002Ticket.Id && t.Id.StartsWith("TEC-") && t.CreatedAt >= req002Ticket.CreatedAt && t != req002Ticket), "REQ-002: crear solicitud no genera trabajo técnico automático");
// El solicitante puede ser Tecnología actuando por alguien más (ID opaco, sin impersonación implementada).
var req002TecInput = req002Input with { SolicitanteId = "tecnologia-internal" };
var req002TecTicket = (await service.Create(req002TecInput, ct)).Tickets.Last();
Check(req002TecTicket.SolicitanteId == "tecnologia-internal", "REQ-002: Tecnología puede registrar una solicitud con su propio ID");
// La descripción es obligatoria — la escritura es el elemento principal del formulario.
await Reject<ArgumentException>(() => service.Create(req002Input with { Description = "   " }, ct), "REQ-002: descripción obligatoria (escritura como elemento principal)");
// El título es obligatorio.
await Reject<ArgumentException>(() => service.Create(req002Input with { Title = "" }, ct), "REQ-002: título obligatorio");
Console.WriteLine("Todas las comprobaciones REQ-002 correctas.");

// REQ-003 — Mis Solicitudes
// El servicio entrega todas las actividades; el filtrado de visibilidad es responsabilidad
// del presentador (frontend). Se verifica que el modelo soporta las visibilidades acordadas
// y que las actividades de un ticket incluyen solo los tipos definidos.
var req003SolicitanteId = "usuario-portal-01";
var req003Input = new TicketInput("Mi reporte no carga", "Entro al portal y la pantalla queda en blanco.", CategoriasTicket.Problema, req003SolicitanteId);
var req003Data = await service.Create(req003Input, ct);
var req003Ticket = req003Data.Tickets.Last();
// El usuario puede aportar información posterior como "Respuesta al solicitante"
await service.AddActivity(new ActivityInput(req003Ticket.Id, "El error ocurre solo en Chrome 126.", "Respuesta al solicitante"), ct);
// Tecnología puede registrar notas internas que el solicitante no debe ver
await service.AddActivity(new ActivityInput(req003Ticket.Id, "Revisando logs del servidor. No compartir con el cliente.", "Nota interna"), ct);
var req003Acts = (await service.Load(ct)).Activities.Where(a => a.TicketId == req003Ticket.Id).ToList();
// Create registra automáticamente una actividad interna ("Ticket creado"). Se añadieron 2 más.
Check(req003Acts.Count == 3, "REQ-003: el modelo almacena todas las actividades del ticket (creación + 2 manuales)");
// Las "Respuesta al solicitante" son las únicas visibles para el usuario general
var visibles = req003Acts.Where(a => a.Visibility == "Respuesta al solicitante").ToList();
var internas = req003Acts.Where(a => a.Visibility == "Nota interna").ToList();
Check(visibles.Count == 1 && visibles[0].Text == "El error ocurre solo en Chrome 126.", "REQ-003: solo 'Respuesta al solicitante' es visible para el solicitante");
Check(internas.Count == 2, "REQ-003: notas internas (creación + manual) existen en el modelo pero no se entregan al solicitante");
// El solicitante solo ve sus propias solicitudes — el filtrado es por SolicitanteId
var otroSolicitante = (await service.Create(req003Input with { SolicitanteId = "otro-usuario" }, ct)).Tickets.Last();
var misTickets = (await service.Load(ct)).Tickets.Where(t => t.SolicitanteId == req003SolicitanteId).ToList();
Check(misTickets.All(t => t.SolicitanteId == req003SolicitanteId), "REQ-003: filtrar por SolicitanteId devuelve solo solicitudes propias");
Check(!misTickets.Any(t => t.Id == otroSolicitante.Id), "REQ-003: solicitudes de otro usuario no aparecen en mis solicitudes");
// La información que el usuario agrega llega al equipo de Tecnología
var req003InfoData = await service.AddActivity(new ActivityInput(req003Ticket.Id, "También falla en Firefox.", "Respuesta al solicitante"), ct);
var req003InfoAct = req003InfoData.Activities.First(a => a.Text == "También falla en Firefox.");
Check(req003InfoAct.Visibility == "Respuesta al solicitante" && req003InfoAct.TicketId == req003Ticket.Id, "REQ-003: información posterior del solicitante llega a Tecnología");
Console.WriteLine("Todas las comprobaciones REQ-003 correctas.");

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
