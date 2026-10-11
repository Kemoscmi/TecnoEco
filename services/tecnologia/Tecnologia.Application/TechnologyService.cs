using Tecnologia.Domain;
namespace Tecnologia.Application;
// REQ-010: metadatos de un adjunto recibido en el cuerpo de la petición
public record AdjuntoInput(string Nombre,string Tipo,long Tamaño);
public record TicketInput(string Title,string Description,string Categoria,string SolicitanteId,List<AdjuntoInput>? Adjuntos=null);
public record ActivityInput(string? TicketId,string Text,string Visibility,List<AdjuntoInput>? Adjuntos=null);
public record StatusInput(string Estado);
public record EditarSolicitudInput(string Title,string Description,string Categoria,string ActorId);
public record EliminarSolicitudInput(string ActorId);
public record RelacionarTrabajoTecnicoInput(string TrabajoTecnicoId,string TipoTrabajoTecnico,string ActorId);
// REQ-005: acciones de asignación. ActorId identifica al miembro de Tecnología que ejecuta la acción.
public record TomarSolicitudInput(string ActorId);
// REQ-012: cierre de solicitud. El mensaje/motivo se publica al solicitante.
public record ResolverInput(string Mensaje);
public record RechazarInput(string Motivo);
public record ReasignarInput(string ActorId,string NuevoResponsableId);
public record ColaboradorInput(string ColaboradorId);
public record Snapshot(List<Ticket> Tickets,List<Activity> Activities,List<Adjunto> Adjuntos);
public record NotificacionesResult(List<Notificacion> Notificaciones);
public class TechnologyService(ITechnologyRepository repo) {
 // Los identificadores son opacos: no normalizar mayúsculas, espacios ni contenido.
 static string Identity(string? value){if(string.IsNullOrWhiteSpace(value)||value.Length>160)throw new ArgumentException("Identificador requerido (máximo 160 caracteres).");return value;}
 static string Text(string? value,int max){if(string.IsNullOrWhiteSpace(value)||value.Trim().Length>max)throw new ArgumentException($"Texto requerido (máximo {max} caracteres).");return value.Trim();}
 static string Choice(string? value,params string[] allowed){if(value is null||!allowed.Contains(value))throw new ArgumentException("Opción no válida.");return value;}
 // REQ-015: genera una notificación sin enviarla por ningún canal (transporte por definir).
 void Notificar(string destinatarioId,string ticketId,string tipo,string mensaje){
  if(string.IsNullOrWhiteSpace(destinatarioId))return;
  repo.Add(new Notificacion{DestinatarioId=destinatarioId,TicketId=ticketId,TipoNotificacion=tipo,Mensaje=mensaje});
 }
 public async Task<Snapshot> Load(CancellationToken ct)=>new(await repo.Tickets(ct),await repo.Activities(ct),await repo.Adjuntos(ct));
 // REQ-010: valida y normaliza el nombre de archivo
 static string FileName(string? value){if(string.IsNullOrWhiteSpace(value)||value.Trim().Length>260)throw new ArgumentException("Nombre de archivo requerido (máximo 260 caracteres).");return value.Trim();}
 static void AddAdjuntos(ITechnologyRepository repo,string ticketId,string? activityId,IEnumerable<AdjuntoInput>? adjuntos,string visibility){
  foreach(var a in adjuntos??[])repo.Add(new Adjunto{TicketId=ticketId,ActivityId=activityId,Nombre=FileName(a.Nombre),Tipo=string.IsNullOrWhiteSpace(a.Tipo)?"application/octet-stream":a.Tipo.Trim(),Tamaño=a.Tamaño,Visibility=visibility,Url=null});
 }
 public async Task<Snapshot> Create(TicketInput input,CancellationToken ct){
  var ticket=new Ticket {Title=Text(input.Title,140),Description=Text(input.Description,10000),Categoria=Choice(input.Categoria,CategoriasTicket.Iniciales.ToArray()),SolicitanteId=Identity(input.SolicitanteId),ResponsableId=null,Estado=EstadosTicket.Recibida};
  repo.Add(ticket);repo.Add(new Activity{TicketId=ticket.Id,Text="Ticket creado. Pendiente de revisión."});
  AddAdjuntos(repo,ticket.Id,null,input.Adjuntos,"publica");
  await repo.Save(ct);return await Load(ct);
 }
 static void ValidarEdicionInicial(Ticket ticket,string actor){
  if(ticket.SolicitanteId!=actor)throw new InvalidOperationException("Solo el solicitante puede modificar o eliminar esta solicitud.");
  if(ticket.Estado!=EstadosTicket.Recibida||ticket.AbandonoRecibidaAt is not null)throw new InvalidOperationException("La solicitud solo puede modificarse o eliminarse mientras está Recibida y no la haya abandonado antes.");
 }
 // REQ-013: el contenido original solo es editable por su solicitante mientras permanece Recibida.
 public async Task<Snapshot> EditarSolicitud(string id,EditarSolicitudInput input,CancellationToken ct){
  var actor=Identity(input.ActorId);var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  ValidarEdicionInicial(ticket,actor);
  ticket.Title=Text(input.Title,140);ticket.Description=Text(input.Description,10000);ticket.Categoria=Choice(input.Categoria,CategoriasTicket.Iniciales.ToArray());
  repo.Add(new Activity{TicketId=id,Text=$"Solicitud editada por {actor}.",Visibility="Nota interna"});
  await repo.Save(ct);return await Load(ct);
 }
 // REQ-013: conservación lógica del Ticket y su trazabilidad; no borra registros físicos.
 public async Task<Snapshot> EliminarSolicitud(string id,EliminarSolicitudInput input,CancellationToken ct){
  var actor=Identity(input.ActorId);var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  ValidarEdicionInicial(ticket,actor);
  ticket.EliminadoPorId=actor;ticket.EliminadoAt=DateTimeOffset.UtcNow;
  repo.Add(new Activity{TicketId=id,Text=$"Solicitud eliminada lógicamente por {actor} el {ticket.EliminadoAt:u}.",Visibility="Nota interna"});
  await repo.Save(ct);return await Load(ct);
 }
 // REQ-014: crea solo la referencia y su actividad; no crea trabajo técnico ni modifica el estado.
 public async Task<Snapshot> RelacionarTrabajoTecnico(string id,RelacionarTrabajoTecnicoInput input,CancellationToken ct){
  var trabajoId=Identity(input.TrabajoTecnicoId);var tipo=TiposTrabajoTecnico.Validar(input.TipoTrabajoTecnico);var actor=Identity(input.ActorId);
  var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.TrabajosTecnicos.Any(t=>t.TrabajoTecnicoId==trabajoId))throw new InvalidOperationException("El trabajo técnico ya está relacionado con esta solicitud.");
  var relacion=new TrabajoTecnicoRelacionado{TicketId=id,TrabajoTecnicoId=trabajoId,TipoTrabajoTecnico=tipo,RelacionadoPorId=actor};
  ticket.TrabajosTecnicos.Add(relacion);repo.Add(relacion);
  repo.Add(new Activity{TicketId=id,Text=$"Trabajo técnico relacionado: {tipo} {trabajoId}, por {actor}.",Visibility="Nota interna"});
  await repo.Save(ct);return await Load(ct);
 }
 // Acción explícita de Tecnología. REQ-004 no define una matriz restrictiva de transiciones.
 public async Task<Snapshot> ChangeStatus(string id,StatusInput input,CancellationToken ct){
  var status=EstadosTicket.Validar(input.Estado);var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Estado!=status){if(ticket.Estado==EstadosTicket.Recibida&&status!=EstadosTicket.Recibida)ticket.AbandonoRecibidaAt=DateTimeOffset.UtcNow;repo.Add(new Activity{TicketId=id,Text=$"Estado actualizado: {ticket.Estado} → {status}."});
   if(status==EstadosTicket.NecesitamosInformacion)Notificar(ticket.SolicitanteId,id,TiposNotificacion.NecesitamosInformacion,"Tecnología necesita información sobre tu solicitud.");
   ticket.Estado=status;await repo.Save(ct);}return await Load(ct);
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
 // REQ-012 — Resolución: requiere mensaje obligatorio visible para el solicitante.
 public async Task<Snapshot> Resolver(string id,ResolverInput input,CancellationToken ct){
  var mensaje=Text(input.Mensaje,10000);
  var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Estado==EstadosTicket.Resuelta)throw new InvalidOperationException("La solicitud ya está resuelta.");
  ticket.Estado=EstadosTicket.Resuelta;
  repo.Add(new Activity{TicketId=id,Text=mensaje,Visibility="Respuesta al solicitante"});
  repo.Add(new Activity{TicketId=id,Text="Estado actualizado: "+EstadosTicket.Resuelta+". Resolución comunicada al solicitante.",Visibility="Nota interna"});
  Notificar(ticket.SolicitanteId,id,TiposNotificacion.SolicitudResuelta,"Tu solicitud ha sido resuelta.");
  await repo.Save(ct);return await Load(ct);
 }
 // REQ-012 — Rechazo: requiere motivo obligatorio visible para el solicitante.
 public async Task<Snapshot> Rechazar(string id,RechazarInput input,CancellationToken ct){
  var motivo=Text(input.Motivo,10000);
  var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Estado==EstadosTicket.Rechazada)throw new InvalidOperationException("La solicitud ya está rechazada.");
  ticket.Estado=EstadosTicket.Rechazada;
  repo.Add(new Activity{TicketId=id,Text=motivo,Visibility="Respuesta al solicitante"});
  repo.Add(new Activity{TicketId=id,Text="Estado actualizado: "+EstadosTicket.Rechazada+". Motivo comunicado al solicitante.",Visibility="Nota interna"});
  Notificar(ticket.SolicitanteId,id,TiposNotificacion.SolicitudRechazada,"Tu solicitud ha sido rechazada.");
  await repo.Save(ct);return await Load(ct);
 }
 // REQ-015: consultar notificaciones de un destinatario (no filtra leídas; el presentador decide).
 public async Task<NotificacionesResult> ObtenerNotificaciones(string destinatarioId,CancellationToken ct){
  var id=Identity(destinatarioId);
  return new(await repo.Notificaciones(id,ct));
 }
 // REQ-015: el destinatario marca una notificación como leída.
 public async Task MarcarLeida(string notificacionId,CancellationToken ct){
  var n=await repo.FindNotificacion(notificacionId,ct)??throw new KeyNotFoundException("Notificación no encontrada.");
  n.Leida=true;await repo.Save(ct);
 }
 public async Task<Snapshot> AddActivity(ActivityInput input,CancellationToken ct){
  var text=Text(input.Text,10000);var visibility=Choice(input.Visibility,"Nota interna","Respuesta al solicitante");
  if(input.TicketId is not null && await repo.Find(input.TicketId,ct) is null)throw new KeyNotFoundException("Ticket no encontrado.");
  if(input.TicketId is null && visibility!="Nota interna")throw new ArgumentException("Las actividades generales son internas.");
  var activity=new Activity{TicketId=input.TicketId,Text=text,Visibility=visibility};
  repo.Add(activity);
  // REQ-010: adjuntos heredan la visibilidad de la actividad (publica | interna)
  if(input.TicketId is not null){var adjVis=visibility=="Respuesta al solicitante"?"publica":"interna";AddAdjuntos(repo,input.TicketId,activity.Id,input.Adjuntos,adjVis);}
  // REQ-015: actividades con visibilidad pública generan notificación al destinatario correcto.
  if(input.TicketId is not null && visibility=="Respuesta al solicitante"){
   var ticketNot=await repo.Find(input.TicketId,ct);
   if(ticketNot is not null){
    if(ticketNot.Estado==EstadosTicket.NecesitamosInformacion)
     Notificar(ticketNot.ResponsableId??ticketNot.SolicitanteId,input.TicketId,TiposNotificacion.UsuarioRespondioAResponsable,"El usuario respondió a la solicitud que esperaba información.");
    else
     Notificar(ticketNot.SolicitanteId,input.TicketId,TiposNotificacion.TecnologiaRespondio,"Tecnología ha respondido a tu solicitud.");
   }
  }
  await repo.Save(ct);return await Load(ct);
 }
}
