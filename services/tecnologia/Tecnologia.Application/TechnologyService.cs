using Tecnologia.Domain;
namespace Tecnologia.Application;
public record TicketInput(string Title,string Description,string Categoria,string SolicitanteId);
public record ActivityInput(string? TicketId,string Text,string Visibility);
public record StatusInput(string Estado);
// REQ-005: acciones de asignación. ActorId identifica al miembro de Tecnología que ejecuta la acción.
public record TomarSolicitudInput(string ActorId);
public record ReasignarInput(string ActorId,string NuevoResponsableId);
public record ColaboradorInput(string ColaboradorId);
public record Snapshot(List<Ticket> Tickets,List<Activity> Activities);
public class TechnologyService(ITechnologyRepository repo) {
 // Los identificadores son opacos: no normalizar mayúsculas, espacios ni contenido.
 static string Identity(string? value){if(string.IsNullOrWhiteSpace(value)||value.Length>160)throw new ArgumentException("Identificador requerido (máximo 160 caracteres).");return value;}
 static string Text(string? value,int max){if(string.IsNullOrWhiteSpace(value)||value.Trim().Length>max)throw new ArgumentException($"Texto requerido (máximo {max} caracteres).");return value.Trim();}
 static string Choice(string? value,params string[] allowed){if(value is null||!allowed.Contains(value))throw new ArgumentException("Opción no válida.");return value;}
 public async Task<Snapshot> Load(CancellationToken ct)=>new(await repo.Tickets(ct),await repo.Activities(ct));
 public async Task<Snapshot> Create(TicketInput input,CancellationToken ct){
  var ticket=new Ticket {Title=Text(input.Title,140),Description=Text(input.Description,10000),Categoria=Choice(input.Categoria,CategoriasTicket.Iniciales.ToArray()),SolicitanteId=Identity(input.SolicitanteId),ResponsableId=null,Estado=EstadosTicket.Recibida};
  repo.Add(ticket);repo.Add(new Activity{TicketId=ticket.Id,Text="Ticket creado. Pendiente de revisión."});await repo.Save(ct);return await Load(ct);
 }
 // Acción explícita de Tecnología. REQ-004 no define una matriz restrictiva de transiciones.
 public async Task<Snapshot> ChangeStatus(string id,StatusInput input,CancellationToken ct){
  var status=EstadosTicket.Validar(input.Estado);var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Estado!=status){repo.Add(new Activity{TicketId=id,Text=$"Estado actualizado: {ticket.Estado} → {status}."});ticket.Estado=status;await repo.Save(ct);}return await Load(ct);
 }
 // REQ-005 — Tomar solicitud: asigna responsable y, si estaba en Recibida, avanza a En proceso.
 // "Tomar fuera de Recibida" queda sin regla definida; esta implementación asigna igualmente sin forzar transición.
 public async Task<Snapshot> TomarSolicitud(string id,TomarSolicitudInput input,CancellationToken ct){
  var actor=Identity(input.ActorId);
  var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  ticket.ResponsableId=actor;
  if(ticket.Estado==EstadosTicket.Recibida){ticket.Estado=EstadosTicket.EnProceso;repo.Add(new Activity{TicketId=id,Text=$"{actor} tomó la solicitud. Estado: Recibida → En proceso."});}
  else{repo.Add(new Activity{TicketId=id,Text=$"{actor} tomó la solicitud."});}
  await repo.Save(ct);return await Load(ct);
 }
 // REQ-005 — Reasignar: cambia responsable principal, conserva estado, registra actor, anterior, nuevo y momento UTC.
 public async Task<Snapshot> Reasignar(string id,ReasignarInput input,CancellationToken ct){
  var actor=Identity(input.ActorId);var nuevo=Identity(input.NuevoResponsableId);
  var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  var anterior=ticket.ResponsableId??"(sin responsable)";
  ticket.ResponsableId=nuevo;
  repo.Add(new Activity{TicketId=id,Text=$"Reasignada por {actor}. Responsable: {anterior} → {nuevo}. {DateTimeOffset.UtcNow:u}"});
  await repo.Save(ct);return await Load(ct);
 }
 // REQ-005 — Agregar colaborador: rechaza duplicados.
 public async Task<Snapshot> AgregarColaborador(string id,ColaboradorInput input,CancellationToken ct){
  var colaboradorId=Identity(input.ColaboradorId);
  var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Colaboradores.Any(c=>c.ColaboradorId==colaboradorId))throw new InvalidOperationException("El colaborador ya está asociado a esta solicitud.");
  ticket.Colaboradores.Add(new TicketColaborador{TicketId=id,ColaboradorId=colaboradorId});
  repo.Add(new Activity{TicketId=id,Text=$"Colaborador agregado: {colaboradorId}."});
  await repo.Save(ct);return await Load(ct);
 }
 public async Task<Snapshot> AddActivity(ActivityInput input,CancellationToken ct){
  var text=Text(input.Text,10000);var visibility=Choice(input.Visibility,"Nota interna","Respuesta al solicitante");
  if(input.TicketId is not null && await repo.Find(input.TicketId,ct) is null)throw new KeyNotFoundException("Ticket no encontrado.");
  if(input.TicketId is null && visibility!="Nota interna")throw new ArgumentException("Las actividades generales son internas.");
  repo.Add(new Activity{TicketId=input.TicketId,Text=text,Visibility=visibility});await repo.Save(ct);return await Load(ct);
 }
}
