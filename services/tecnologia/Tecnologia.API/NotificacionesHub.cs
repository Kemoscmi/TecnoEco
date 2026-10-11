using Microsoft.AspNetCore.SignalR;
namespace Tecnologia.API;
// REQ-015: hub SignalR para notificaciones en tiempo real.
// El cliente se conecta con ?destinatarioId=<id> y queda suscrito a su grupo personal.
public class NotificacionesHub : Hub {
 public override async Task OnConnectedAsync() {
  var destinatarioId = Context.GetHttpContext()?.Request.Query["destinatarioId"].FirstOrDefault();
  if (!string.IsNullOrWhiteSpace(destinatarioId))
   await Groups.AddToGroupAsync(Context.ConnectionId, GrupoDeUsuario(destinatarioId));
  await base.OnConnectedAsync();
 }
 public static string GrupoDeUsuario(string destinatarioId) => "u:" + destinatarioId;
}
