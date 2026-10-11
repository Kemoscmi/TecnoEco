import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr'

// REQ-015: crea una conexión al hub de notificaciones para el destinatario dado.
// La conexión se reconecta automáticamente con back-off exponencial.
export function crearConexionNotificaciones(destinatarioId: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl('/hubs/notificaciones?destinatarioId=' + encodeURIComponent(destinatarioId))
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
