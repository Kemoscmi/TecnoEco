# REQ-001 — Modelo base de Solicitudes

Implementación de [#2](https://github.com/Kemoscmi/TecnoEco/issues/2), dentro de la [Épica #1](https://github.com/Kemoscmi/TecnoEco/issues/1). Las decisiones confirmadas para REQ-001 sustituyen los campos del modelo inicial del prototipo.

## Modelo y contratos

`Ticket` representa una solicitud dirigida a Tecnología; no es un Bug. Conserva `Id`, `Title`, `Description` y `CreatedAt` (UTC), evitando una traducción general del código existente.

- `SolicitanteId`: obligatorio, identificador opaco de texto de hasta 160 caracteres. Identifica a quien crea y necesita la atención. No hay `CreadoPorId` ni captura en nombre de terceros.
- `ResponsableId`: identificador opaco de texto de hasta 160 caracteres, opcional; ausencia representada mediante `null`. Todo Ticket nuevo se crea con este valor en `null`; REQ-005 definirá la asignación posterior.
- `Colaboradores`: relación independiente `TicketColaborador`, con clave compuesta `(TicketId, ColaboradorId)`. Permite cero o varios colaboradores y evita duplicados por Ticket. No se agregan operaciones ni interfaz de gestión de colaboradores.
- `Categoria`: texto de hasta 40 caracteres. Catálogo inicial: Problema, Consulta, Solicitud de ayuda, Sugerencia / mejora. Las constantes describen el catálogo actual y pueden evolucionar; no hay ENUM SQL ni taxonomía inmutable.
- `Estado`: texto de hasta 40 caracteres, inicialmente Recibida. El vocabulario contiene únicamente Recibida, En proceso, Necesitamos información, Resuelta y Rechazada. REQ-004 valida los cambios explícitos; no define una matriz restrictiva de transiciones.

Los identificadores externos no se normalizan ni representan nombres de personas. Su collation SQL es `utf8mb4_bin` para distinguir mayúsculas. No se crean tablas de usuarios, claves externas a identidad, perfiles persistidos, autenticación ni permisos. Los perfiles conceptuales continúan siendo Cliente, Soporte y Development; la integración definitiva sigue pendiente.

Se eliminan `Type`, `Origin`, `Priority`, `Requester`, `Assignee` y `Status`. No hay prioridades ni SLA.

`EventoTicket` prepara Ticket relacionado, actor, fecha UTC y tipo de evento. Normaliza el instante a UTC. No está mapeado en EF y no se genera, persiste ni consulta. El catálogo y mecanismo de auditoría corresponden a REQ-011. `Activity` sigue siendo una actividad heredada del prototipo, no la bitácora definitiva.

## Adaptación del prototipo

Se actualizan los DTO, servicio, repositorio, tipos TypeScript y consumidores existentes para usar el nuevo modelo. Se conservan las rutas de la API; el cuerpo del PATCH `/tickets/{id}/status` ahora contiene `estado`. No se mantienen alias del contrato antiguo.

La creación y actividades preexistentes siguen siendo mecanismos de demostración en Development. REQ-004 incorpora cambios de estado explícitos, pero no autenticación/autorización ni una matriz de transiciones. Los campos de identidad de la pantalla son IDs de prueba, no una integración de identidad ni autorización para actuar por otra persona. Los controles, conteos y vistas existentes solo se adaptan al vocabulario; no se entregan los requerimientos de dashboard, listado o detalle.

El almacenamiento demo usa la nueva clave `tecnologia-ecosystem.demo.req001.v2`; no importa datos incompatibles ni borra la clave anterior. El modo API no importa datos demo.

## Esquema SQL

`database/001_tecnologia.sql` sigue siendo la fuente del esquema; EF solo lo mapea. La tabla `tecnologia_tickets` contiene los campos nuevos y se agrega `tecnologia_ticket_colaboradores`, con clave compuesta y FK restrictiva hacia Ticket. `tecnologia_actividades` permanece como soporte heredado. No se crean migrations ni se usa `EnsureCreated`.

**El script es para inicializar una base vacía.** `CREATE TABLE IF NOT EXISTS` no actualiza tablas ya creadas. Una base del prototipo con las columnas anteriores debe respaldarse y reinicializarse explícitamente antes de usar esta versión. No se proporciona conversión automática de nombres de personas a IDs externos ni se destruyen datos existentes al ejecutar el script. Docker solo inicializa SQL cuando su volumen está vacío. Esta implementación no modifica la base local del usuario.

## Verificación

```powershell
dotnet build tecnologia-ecosystem.sln --no-restore
dotnet run --project services/tecnologia/Tecnologia.Tests --no-build
# Desde frontends/ecosystem:
npm.cmd run build
```

La suite ejecutable verifica contratos, validación de entradas y metadatos EF. Para integración real, configurar `REQ001_TEST_SERVER` con una conexión a una instancia de pruebas MySQL/MariaDB y permisos de creación/eliminación de bases. La suite crea una base exclusiva `req001_test_<guid>`, ejecuta el SQL versionado dos veces, comprueba persistencia y restricciones, y elimina solo esa base en `finally`. No utiliza `tecnologia_db`. Si no se configura la variable, informa explícitamente que omite la integración SQL.

## Pendiente fuera de REQ-001

Integración definitiva de identidad/perfiles; reglas de asignación y colaboradores (REQ-005); auditoría completa (REQ-011); validaciones de resolución/rechazo (REQ-012); funcionalidades visuales, adjuntos, notificaciones y trabajo técnico de los requerimientos posteriores.

Este primer PR usa `main` como base porque no existe `develop`. La estrategia de integración debe formalizarse antes de los siguientes requerimientos.
