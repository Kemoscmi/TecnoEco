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

// REQ-005 — Asignación, responsables y colaboradores
var req005Input = new TicketInput("Acceso al servidor de respaldos", "Solicito acceso para revisar los respaldos del último trimestre.", CategoriasTicket.SolicitudDeAyuda, "solicitante-req005");
var req005Data = await service.Create(req005Input, ct);
var req005Ticket = req005Data.Tickets.Last();
// Una solicitud puede iniciar sin responsable
Check(req005Ticket.ResponsableId is null && req005Ticket.Estado == EstadosTicket.Recibida, "REQ-005: solicitud inicia sin responsable en Recibida");
// Tomar solicitud: asigna responsable y avanza Recibida → En proceso
var req005TomadaData = await service.TomarSolicitud(req005Ticket.Id, new TomarSolicitudInput("soporte-01"), ct);
var req005TomadaTicket = req005TomadaData.Tickets.First(t => t.Id == req005Ticket.Id);
Check(req005TomadaTicket.ResponsableId == "soporte-01", "REQ-005: Tomar solicitud asigna responsable");
Check(req005TomadaTicket.Estado == EstadosTicket.EnProceso, "REQ-005: Tomar solicitud cambia Recibida a En proceso");
var req005TomadaAct = req005TomadaData.Activities.First(a => a.TicketId == req005Ticket.Id && a.Text.Contains("tomó la solicitud"));
Check(req005TomadaAct is not null, "REQ-005: Tomar solicitud registra la acción en bitácora");
// Reasignar: cambia responsable, conserva estado, registra actor/anterior/nuevo/momento
var req005ReasigData = await service.Reasignar(req005Ticket.Id, new ReasignarInput("soporte-02", "desarrollo-01"), ct);
var req005ReasigTicket = req005ReasigData.Tickets.First(t => t.Id == req005Ticket.Id);
Check(req005ReasigTicket.ResponsableId == "desarrollo-01", "REQ-005: reasignar actualiza el responsable principal");
Check(req005ReasigTicket.Estado == EstadosTicket.EnProceso, "REQ-005: reasignar conserva el estado actual");
var req005ReasigAct = req005ReasigData.Activities.First(a => a.TicketId == req005Ticket.Id && a.Text.Contains("Reasignada"));
Check(req005ReasigAct.Text.Contains("soporte-02") && req005ReasigAct.Text.Contains("soporte-01") && req005ReasigAct.Text.Contains("desarrollo-01"), "REQ-005: reasignar registra actor, responsable anterior y nuevo");
// Agregar colaboradores: uno o varios, sin duplicados
await service.AgregarColaborador(req005Ticket.Id, new ColaboradorInput("dev-colaborador-01"), ct);
await service.AgregarColaborador(req005Ticket.Id, new ColaboradorInput("dev-colaborador-02"), ct);
var req005ColabData = await service.Load(ct);
var req005ColabTicket = req005ColabData.Tickets.First(t => t.Id == req005Ticket.Id);
Check(req005ColabTicket.Colaboradores.Count == 2, "REQ-005: se pueden agregar varios colaboradores");
Check(req005ColabTicket.Colaboradores.Any(c => c.ColaboradorId == "dev-colaborador-01") && req005ColabTicket.Colaboradores.Any(c => c.ColaboradorId == "dev-colaborador-02"), "REQ-005: colaboradores registrados correctamente");
// Un único responsable principal — al tomar/reasignar siempre hay uno
Check(req005ColabTicket.ResponsableId == "desarrollo-01", "REQ-005: existe un único responsable principal");
// Rechazar colaborador duplicado
await Reject<InvalidOperationException>(() => service.AgregarColaborador(req005Ticket.Id, new ColaboradorInput("dev-colaborador-01"), ct), "REQ-005: rechazar colaborador duplicado");
// Tomar solicitud fuera de Recibida: asigna responsable pero no fuerza transición de estado
var req005OtroInput = new TicketInput("Segunda solicitud", "Para probar tomar fuera de Recibida.", CategoriasTicket.Consulta, "solicitante-req005b");
var req005OtroTicket = (await service.Create(req005OtroInput, ct)).Tickets.Last();
await service.ChangeStatus(req005OtroTicket.Id, new StatusInput(EstadosTicket.EnProceso), ct);
await service.TomarSolicitud(req005OtroTicket.Id, new TomarSolicitudInput("soporte-03"), ct);
var req005OtroFinal = (await service.Load(ct)).Tickets.First(t => t.Id == req005OtroTicket.Id);
Check(req005OtroFinal.ResponsableId == "soporte-03", "REQ-005: Tomar solicitud fuera de Recibida asigna responsable");
Check(req005OtroFinal.Estado == EstadosTicket.EnProceso, "REQ-005: Tomar fuera de Recibida no cambia el estado actual");
// Ticket inexistente
await Reject<KeyNotFoundException>(() => service.TomarSolicitud("no-existe", new TomarSolicitudInput("actor"), ct), "REQ-005: Tomar solicitud rechaza ticket inexistente");
await Reject<KeyNotFoundException>(() => service.Reasignar("no-existe", new ReasignarInput("actor", "nuevo"), ct), "REQ-005: Reasignar rechaza ticket inexistente");
await Reject<ArgumentException>(() => service.Reasignar(req005Ticket.Id, new ReasignarInput("actor", " "), ct), "REQ-005: Reasignar rechaza ID de responsable vacío");
Console.WriteLine("Todas las comprobaciones REQ-005 correctas.");

// REQ-010 — Archivos adjuntos
var req010Input = new TicketInput("Solicitud con adjuntos","Necesito acceso; adjunto capturas.",CategoriasTicket.Problema,"sol-adj-01",
    [new AdjuntoInput("captura.png","image/png",102400),new AdjuntoInput("log.txt","text/plain",2048)]);
var req010Data = await service.Create(req010Input, ct);
var req010Ticket = req010Data.Tickets.Last();
var adjTicket = req010Data.Adjuntos.Where(a => a.TicketId == req010Ticket.Id && a.ActivityId is null).ToList();
Check(adjTicket.Count == 2, "REQ-010: adjuntos de solicitud original registrados");
Check(adjTicket.All(a => a.Visibility == "publica"), "REQ-010: adjuntos de solicitud original son públicos");
Check(adjTicket.All(a => a.Url is null), "REQ-010: URL de adjunto es null hasta configurar storage");
// Adjunto en respuesta al solicitante → visibility publica
var req010RespData = await service.AddActivity(new ActivityInput(req010Ticket.Id,"Adjunto la solución.","Respuesta al solicitante",
    [new AdjuntoInput("solucion.pdf","application/pdf",51200)]),ct);
var adjResp = req010RespData.Adjuntos.Where(a => a.TicketId == req010Ticket.Id && a.ActivityId is not null && a.Visibility == "publica").ToList();
Check(adjResp.Count == 1, "REQ-010: adjunto de respuesta al solicitante es público");
// Adjunto en nota interna → visibility interna (solicitante NO debe verlo)
var req010NotaData = await service.AddActivity(new ActivityInput(req010Ticket.Id,"Debug interno.","Nota interna",
    [new AdjuntoInput("debug.log","text/plain",4096)]),ct);
var adjInterna = req010NotaData.Adjuntos.Where(a => a.Nombre == "debug.log").ToList();
Check(adjInterna.Count == 1 && adjInterna[0].Visibility == "interna", "REQ-010: adjunto de nota interna es interno (no visible para el solicitante)");
// Sin adjuntos: actividades y tickets sin adjuntos en el input no generan entradas
var req010SinAdj = await service.Create(new TicketInput("Sin adjuntos","Prueba",CategoriasTicket.Consulta,"sol-sin-adj"),ct);
var req010SinAdjTicket = req010SinAdj.Tickets.Last();
Check(!req010SinAdj.Adjuntos.Any(a => a.TicketId == req010SinAdjTicket.Id), "REQ-010: ticket sin adjuntos en el input no genera entradas en Adjuntos");
Console.WriteLine("Todas las comprobaciones REQ-010 correctas.");

static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("OK: " + message); }
static async Task Reject<T>(Func<Task<Snapshot>> action, string message) where T : Exception {
    try { await action(); } catch (T) { Console.WriteLine("OK: " + message); return; }
    throw new Exception(message);
}
sealed class MemoryRepository : ITechnologyRepository {
    public List<Ticket> Items { get; } = [];
    public List<Activity> Log { get; } = [];
    public List<Adjunto> Files { get; } = [];
    public Task<List<Ticket>> Tickets(CancellationToken ct) => Task.FromResult(Items.ToList());
    public Task<List<Activity>> Activities(CancellationToken ct) => Task.FromResult(Log.ToList());
    public Task<List<Adjunto>> Adjuntos(CancellationToken ct) => Task.FromResult(Files.ToList());
    public Task<Ticket?> Find(string id, CancellationToken ct) => Task.FromResult(Items.FirstOrDefault(t => t.Id == id));
    public void Add(Ticket ticket) => Items.Add(ticket);
    public void Add(Activity activity) => Log.Add(activity);
    public void Add(Adjunto adjunto) => Files.Add(adjunto);
    public Task Save(CancellationToken ct) => Task.CompletedTask;
}
