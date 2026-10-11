# REQ-014 — Relación Solicitud ↔ Trabajo técnico

Implementación de [#15](https://github.com/Kemoscmi/TecnoEco/issues/15). Prepara únicamente referencias entre una Solicitud (`Ticket`) y trabajo técnico, sin crear módulos para Bug, Historia de usuario, Tarea técnica o Mejora.

## Representación mínima

`TrabajoTecnicoRelacionado` contiene `TicketId`, `TrabajoTecnicoId` opaco, `TipoTrabajoTecnico`, `RelacionadoPorId` y fecha UTC. La clave compuesta `(TicketId, TrabajoTecnicoId)` permite varios trabajos por Solicitud y permite que el mismo identificador técnico esté relacionado con varias Solicitudes; además evita duplicar una misma relación en una Solicitud.

Los únicos tipos conceptuales aceptados son Bug, Historia de usuario, Tarea técnica y Mejora. No representan entidades locales ni una taxonomía definitiva de futuros módulos.

Tecnología crea una relación explícita mediante `POST /api/tecnologia/tickets/{id}/trabajos-tecnicos`. La acción registra una actividad interna compatible con la bitácora existente, conservando actor, tipo e identificador. No modifica el estado de la Solicitud.

## Límites

Crear una Solicitud no crea trabajo técnico. No existe una operación de finalización de trabajo técnico y, por tanto, no hay automatización que resuelva una Solicitud. Tampoco se implementan creación, edición, eliminación, sincronización externa ni ciclos de vida de Bug, Historia de usuario, Tarea técnica o Mejora.

La identidad del actor sigue siendo un ID recibido por el contrato de desarrollo. La autenticación, autorización y validación contra una fuente definitiva de trabajo técnico permanecen pendientes.
