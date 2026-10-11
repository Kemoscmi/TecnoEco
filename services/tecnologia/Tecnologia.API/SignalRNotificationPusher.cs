using Microsoft.AspNetCore.SignalR;
using Tecnologia.Application;
using Tecnologia.Domain;
namespace Tecnologia.API;
public class SignalRNotificationPusher(IHubContext<NotificacionesHub> hub) : INotificationPusher {
 public Task Push(Notificacion n, CancellationToken ct) =>
  hub.Clients.Group(NotificacionesHub.GrupoDeUsuario(n.DestinatarioId))
     .SendAsync("notificacion", n, ct);
}
