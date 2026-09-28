using Tecnologia.Domain;
namespace Tecnologia.Application;
public record TicketInput(string Title,string Description,string Type,string Origin,string Priority,string Requester,string Assignee);
public record ActivityInput(string? TicketId,string Text,string Visibility);
public record StatusInput(string Status);
public record Snapshot(List<Ticket> Tickets,List<Activity> Activities);
public class TechnologyService(ITechnologyRepository repo) {
 static string Text(string? value,int max){if(string.IsNullOrWhiteSpace(value)||value.Trim().Length>max)throw new ArgumentException($"Texto requerido (máximo {max} caracteres).");return value.Trim();}
 static string Choice(string? value,params string[] allowed){if(value is null||!allowed.Contains(value))throw new ArgumentException("Opción no válida.");return value;}
 public async Task<Snapshot> Load(CancellationToken ct)=>new(await repo.Tickets(ct),await repo.Activities(ct));
 public async Task<Snapshot> Create(TicketInput input,CancellationToken ct){
  var ticket=new Ticket {Title=Text(input.Title,140),Description=Text(input.Description,10000),Type=Choice(input.Type,"Bug","Soporte","Historia de usuario","Tarea técnica"),Origin=Choice(input.Origin,"Cliente","Interno"),Priority=Choice(input.Priority,"Alta","Media","Baja"),Requester=Text(input.Requester,160),Assignee=Text(string.IsNullOrWhiteSpace(input.Assignee)?"Por asignar":input.Assignee,160)};
  repo.Add(ticket);repo.Add(new Activity{TicketId=ticket.Id,Text="Ticket creado. Pendiente de revisión."});await repo.Save(ct);return await Load(ct);
 }
 public async Task<Snapshot> ChangeStatus(string id,StatusInput input,CancellationToken ct){
  var status=Choice(input.Status,"Nuevo","En proceso","En QA","Resuelto");var ticket=await repo.Find(id,ct)??throw new KeyNotFoundException("Ticket no encontrado.");
  if(ticket.Status!=status){repo.Add(new Activity{TicketId=id,Text=$"Estado actualizado: {ticket.Status} → {status}."});ticket.Status=status;await repo.Save(ct);}return await Load(ct);
 }
 public async Task<Snapshot> AddActivity(ActivityInput input,CancellationToken ct){
  var text=Text(input.Text,10000);var visibility=Choice(input.Visibility,"Nota interna","Respuesta al solicitante");
  if(input.TicketId is not null && await repo.Find(input.TicketId,ct) is null)throw new KeyNotFoundException("Ticket no encontrado.");
  if(input.TicketId is null && visibility!="Nota interna")throw new ArgumentException("Las actividades generales son internas.");
  repo.Add(new Activity{TicketId=input.TicketId,Text=text,Visibility=visibility});await repo.Save(ct);return await Load(ct);
 }
}
