# REQ-013 — Edición y eliminación de Solicitudes

Implementación de [#14](https://github.com/Kemoscmi/TecnoEco/issues/14). El solicitante puede editar el título, descripción y categoría de su Ticket únicamente mientras conserva el estado `Recibida`.

La edición y eliminación usan el identificador del solicitante como actor del contrato del prototipo. Si el Ticket abandonó `Recibida`, el contenido original no se modifica ni elimina, incluso si una transición posterior lo devuelve a `Recibida`; la información posterior se agrega mediante los mecanismos existentes de comentarios o adjuntos.

La eliminación es lógica. Se conservan `EliminadoAt` en UTC y `EliminadoPorId`, además de una actividad interna. Los repositorios de uso normal excluyen Tickets eliminados de los resultados activos, por lo que dejan de aparecer para el solicitante.

No hay autenticación ni autorización definitiva: el actor procede del cuerpo de la petición y la comparación con `SolicitanteId` expresa la regla funcional, no un control de seguridad final. No se crean permisos adicionales para Tecnología. La retención, restauración y consultas internas de eliminadas permanecen pendientes.
