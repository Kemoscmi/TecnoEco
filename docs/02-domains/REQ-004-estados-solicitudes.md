# REQ-004 — Estados y ciclo de vida de Solicitudes

Implementación de [#5](https://github.com/Kemoscmi/TecnoEco/issues/5), dependiente del modelo de [REQ-001](REQ-001-modelo-base-solicitudes.md).

## Estados

El único vocabulario permitido es `Recibida`, `En proceso`, `Necesitamos información`, `Resuelta` y `Rechazada`. Todo Ticket nuevo se crea en `Recibida`.

Consultar un Ticket no modifica su estado. Un cambio requiere la acción explícita de Tecnología mediante el endpoint existente `PATCH /api/tecnologia/tickets/{id}/status`, cuyo cuerpo contiene `estado`. La validación centralizada rechaza cualquier otro valor.

Tecnología puede cambiar explícitamente una solicitud a `Necesitamos información`. Una respuesta del solicitante se registra como actividad heredada y conserva el estado; Tecnología decide explícitamente cuándo cambiarla a `En proceso`.

No se define una matriz restrictiva de transiciones porque el Issue no la establece. Tampoco existe un modelo de trabajo técnico en esta entrega; por tanto, no hay evento que pueda resolver una solicitud automáticamente.

## Límites actuales

No hay autenticación ni autorización. El endpoint representa una acción de Tecnología solo por contrato y uso previsto; no verifica actor, perfil ni permiso. REQ-005 definirá asignación, REQ-011 la auditoría definitiva y REQ-012 las validaciones completas de resolución y rechazo.

Las actividades registradas durante un cambio continúan siendo compatibles con el prototipo y no sustituyen la bitácora definitiva. El modo demostración conserva el mismo comportamiento: actualiza el estado únicamente mediante la acción explícita de cambio.
