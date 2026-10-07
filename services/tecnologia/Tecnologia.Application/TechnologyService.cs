using Tecnologia.Domain;
namespace Tecnologia.Application;
public record TicketInput(string Title,string Description,string Categoria,string SolicitanteId,string? ResponsableId = null);
public record ActivityInput(string? TicketId,string Text,string Visibility);
public record StatusInput(string Estado);
public record Snapshot(List<Ticket> Tickets,List<Activity> Activities);
public class TechnologyService(ITechnologyRepository repo) {
 // Los identificadores son opacos: no normalizar mayúsculas, espacios ni contenido.
 static string Identity(string? value){if(string.IsNullOrWhiteSpace(value)||value.Length>160)throw new ArgumentException("Identificador requerido (máximo 160 caracteres).");return value;}
 static string Text(string? value,int max){if(string.IsNullOrWhiteSpace(value)||value.Trim().Length>max)throw new ArgumentException($"Texto requerido (máximo {max} caracteres).");return value.Trim();}
 static string Choice(string? value,params string[] allowed){if(value is null||!allowed.Contains(value))throw new ArgumentException("Opción no válida.");return value;}
 public async Task<Snapshot> Load(CancellationToken ct)=>new(await repo.Tickets(ct),await repo.Activities(ct));
 public async Task<Snapshot> Create(TicketInput input,CancellationToken ct){
  var ticket=new Ticket {Title=Text(input.Title,140),Description=Text(input.Description,10000),Categoria=Choice(input.Categoria,CategoriasTicket.Iniciales.ToArray()),SolicitanteId=Identity(input.SolicitanteId),ResponsableId=input.ResponsableId is null ? null : Identity(input.ResponsableId)};
  repo.Add(ticket);repo.Add(new Activity{TicketId=ticket.Id,Text="Ticket creado. Pendiente de revisión."});await repo.Save(ct);return await Load(ct);
 }
 // Adaptación del prototipo; no define ni valida transiciones de REQ-004.
 public async Task<Snapshot> ChangeStatus(string id,StatusInput input,CancellationToken ct){
  var status=Choice(input.Estado,EstadosTicket.Todos.ToArray());var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Estado!=status){repo.Add(new Activity{TicketId=id,Text=$"Estado actualizado: {ticket.Estado} → {status}."});ticket.Estado=status;await repo.Save(ct);}return await Load(ct);
 }
 public async Task<Snapshot> AddActivity(ActivityInput input,CancellationToken ct){
  var text=Text(input.Text,10000);var visibility=Choice(input.Visibility,"Nota interna","Respuesta al solicitante");
  if(input.TicketId is not null && await repo.Find(input.TicketId,ct) is null)throw new KeyNotFoundException("Ticket no encontrado.");
  if(input.TicketId is null && visibility!="Nota interna")throw new ArgumentException("Las actividades generales son internas.");
  repo.Add(new Activity{TicketId=input.TicketId,Text=text,Visibility=visibility});await repo.Save(ct);return await Load(ct);
 }
}
