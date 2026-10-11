using Tecnologia.Domain;
namespace Tecnologia.Application;
// REQ-015: contrato para enviar notificaciones en tiempo real.
// La implementación concreta (SignalR, correo, etc.) vive en la capa de infraestructura/API.
public interface INotificationPusher {
 Task Push(Notificacion notificacion, CancellationToken ct);
}
// Implementación nula para entornos sin transporte configurado (tests, CI).
public class NullNotificationPusher : INotificationPusher {
 public Task Push(Notificacion notificacion, CancellationToken ct) => Task.CompletedTask;
}
