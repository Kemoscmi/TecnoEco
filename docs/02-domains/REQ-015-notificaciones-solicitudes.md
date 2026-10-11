# REQ-015 — Notificaciones de Solicitudes

Implementa: GitHub issue #16 — parte de la épica #1 (Gestión de Solicitudes de Tecnología).

## Objetivo

Preparar la infraestructura de notificaciones para eventos relevantes al usuario, sin comunicar movimientos internos. Los canales (interno en TecnoEco/DrMax y correo electrónico) quedan listos para integrar cuando se defina la infraestructura de transporte.

## Diseño

La entidad `Notificacion` almacena el registro de cada evento notificable. No hay envío real aún: el transporte (SignalR, correo, integración DrMax) se conectará cuando esté definido.

### Eventos que generan notificaciones

| Evento | Destinatario | Tipo |
|--------|-------------|------|
| Estado cambia a «Necesitamos información» | Solicitante | `NecesitamosInformacion` |
| Resolver solicitud | Solicitante | `SolicitudResuelta` |
| Rechazar solicitud | Solicitante | `SolicitudRechazada` |
| «Respuesta al solicitante» fuera de NecesitamosInformacion | Solicitante | `TecnologiaRespondio` |
| «Respuesta al solicitante» mientras estado = NecesitamosInformacion | Responsable | `UsuarioRespondioAResponsable` |

### Eventos que NO generan notificaciones

- Asignaciones (`TomarSolicitud`, `Reasignar`)
- Colaboradores (`AgregarColaborador`)
- Notas internas
- Trabajos técnicos relacionados
- Cambios de estado que no sean NecesitamosInformacion

### Invariantes

- Respuesta del usuario mientras la solicitud está en «Necesitamos información» notifica al responsable y **no cambia el estado**.
- `DestinatarioId` vacío o nulo no genera notificación (guarda silencio en lugar de fallar).
- La tabla `tecnologia_notificaciones` tiene índice en `DestinatarioId` para consultas eficientes.

## API

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/tecnologia/notificaciones?destinatarioId={id}` | Todas las notificaciones del destinatario (no filtra leídas) |
| `PATCH` | `/api/tecnologia/notificaciones/{id}/leer` | Marca una notificación como leída |

## Pendiente (infraestructura por definir)

- Transporte interno (SignalR o integración con plataforma DrMax).
- Envío de correo electrónico (SMTP/proveedor).
- Destinatario cuando no hay responsable asignado: acordar con el área antes de implementar.
- Identidad/contacto de destinatarios: los IDs son opacos; el transporte necesitará resolver el canal real.
