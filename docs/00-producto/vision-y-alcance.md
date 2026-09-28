# TecnoEco: visión y alcance

`tecnologia-ecosystem` / **TecnoEco** nace como un prototipo independiente para diseñar y validar un futuro **Módulo de Tecnología** que podría proponerse e integrarse posteriormente al ecosistema principal de DrMax. Evolucionará conforme validemos el prototipo e identifiquemos necesidades reales.

## Visión del producto

El objetivo no es únicamente crear un sistema de tickets. La visión es centralizar progresivamente solicitudes de clientes e internas, soporte de TI, incidencias, consultas, sugerencias y mejoras, gestión del trabajo de desarrollo, bugs, tareas técnicas, historias de usuario, épicas, QA, bitácora y trazabilidad, soluciones/base de conocimiento, usuarios, perfiles y permisos, y seguimiento de solicitudes por parte del cliente.

Actualmente parte del trabajo técnico se administra mediante GitHub Issues. Durante el desarrollo del prototipo seguiremos utilizándolo como backlog de requerimientos, ideas, funcionalidades, bugs del prototipo, mejoras, decisiones pendientes, preguntas funcionales y funcionalidades futuras. Ese flujo también permitirá aprender para diseñar nuestro propio sistema de gestión de trabajo. Los requerimientos se crearán y refinarán uno por uno; este documento no implica crear Issues en bloque.

La visión futura es que el Módulo de Tecnología sea el sistema principal para administrar el proceso y la trazabilidad del trabajo de Tecnología. No pretende sustituir GitHub como repositorio de código, branches, commits o Pull Requests. La integración futura con DrMax y las posibles integraciones con GitHub siguen sin definirse.

## Alcance implementado

Estado: `IMPLEMENTADO` para el prototipo técnico descrito en el [roadmap](roadmap.md), no para toda la visión de producto. Existe un flujo Vue/Vite → YARP → .NET API → Application → Infrastructure/EF Core → MariaDB/MySQL, además del modo demo local del frontend.

El frontend actual es una vista del equipo interno. Autenticación, aislamiento entre clientes, permisos, portal del cliente, adjuntos, notificaciones y casos/evidencias QA siguen pendientes. La clasificación de visibilidad de notas no equivale a un control de acceso. Los tipos y estados del prototipo no fijan el modelo funcional definitivo.

## Solicitudes y trabajo técnico

Estado del modelo funcional: `EN DEFINICIÓN`.

Una solicitud de un cliente o usuario no debe convertirse automáticamente en un bug. El usuario debe poder explicar un problema, consulta, necesidad o sugerencia/mejora sin conocimientos técnicos. Tecnología analiza después la solicitud, que podría resolverse mediante soporte, generar un bug, una tarea técnica o una mejora/historia, o relacionarse con trabajo técnico existente.

Varias solicitudes podrían relacionarse con un mismo trabajo técnico. Ejemplo conceptual:

```text
Solicitud cliente A ─┐
                    ├── Bug
Solicitud cliente B ─┘
```

El ejemplo expresa una posibilidad; la cardinalidad y la implementación de la relación no son definitivas.

## Experiencia del cliente

Se contempla que el cliente cree solicitudes, aporte información/evidencias, consulte el avance, responda cuando Tecnología solicite información y conozca cuándo su solicitud fue resuelta. Los estados visibles deben ser comprensibles y pueden diferir de los estados técnicos internos. Los estados definitivos están `EN DEFINICIÓN`.

## Desarrollo

El módulo deberá apoyar épicas, historias de usuario, bugs, tareas técnicas, tablero, QA y trazabilidad. Una épica o cualquier trabajo relevante debería conservar el historial de su proceso. Este alcance está `DISEÑADO / PENDIENTE`; las reglas y el flujo definitivo de QA siguen `EN DEFINICIÓN`.

## Soporte de TI

El módulo también debe apoyar Soporte/Operaciones de TI: problemas de acceso, configuración, consultas, incidencias, mantenimiento y otras actividades técnicas. No toda solicitud de soporte debe generar trabajo de desarrollo. El alcance ampliado de soporte está `DISEÑADO / PENDIENTE`.

## Bitácora y trazabilidad

La trazabilidad es central: se busca reconstruir qué ocurrió durante el ciclo de vida de una solicitud, trabajo técnico, bug, historia o épica. Se contempla progresivamente registrar creación, cambios relevantes, actividades, comentarios, responsables, evidencias, QA y resolución/cierre. El prototipo registra actividades y cambios de estado; la estructura definitiva de auditoría está `EN DEFINICIÓN`.

## Base de conocimiento

Estado: `DISEÑADO / PENDIENTE`.

Se contempla una base de soluciones/conocimiento para conservar problemas conocidos, procedimientos y soluciones reutilizables.

## Asistente virtual

Estado: `VISIÓN FUTURA`.

Se contempla atender inicialmente consultas de clientes utilizando la base de conocimiento/documentación, resolver consultas sencillas antes de requerir intervención humana y escalar el caso cuando no pueda resolverlo.

El asistente **no existe actualmente**, su arquitectura no está definida, no se ha seleccionado tecnología, modelo ni proveedor, y no forma parte del alcance implementado.

## Decisiones pendientes y documentación

Roles, permisos, estados, SLA, reglas de asignación, estructura de empresas/clientes, cardinalidades del nuevo modelo, automatizaciones, arquitectura y proveedor/modelo de IA, integración definitiva con GitHub, flujo definitivo de QA y esquema de notificaciones permanecen `EN DEFINICIÓN`.

GitHub Issues será la documentación viva. `/docs` consolidará visión, arquitectura confirmada, modelo funcional y reglas acordadas, decisiones arquitectónicas, guías técnicas y estrategia futura de integración con DrMax cuando alcancen suficiente estabilidad. No se documentan especificaciones inventadas como acuerdos.

El esquema actual lo crea `database/001_tecnologia.sql` y EF Core lo mapea; no se usa EF Core Code First para crear las tablas. Los ADR se redactarán cuando se documenten conscientemente contexto, decisión, alternativas y consecuencias, no por la sola existencia de una tecnología.
