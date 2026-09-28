# Roadmap de TecnoEco

Etapas orientativas sin fechas ni compromisos temporales. El alcance evolucionará con la validación del prototipo y las necesidades reales. `DISEÑADO / PENDIENTE` identifica un alcance conceptual previsto, no reglas o arquitectura definitivas.

## Etapa 0 — Prototipo técnico

Estado: `IMPLEMENTADO`.

Flujo validado: Vue/Vite → YARP → .NET API → Application → Infrastructure/EF Core → MariaDB/MySQL.

Validación E2E ya realizada, según el contexto de validación del proyecto:

- Lectura real desde BD.
- Creación de ticket.
- Persistencia después de recarga.
- Cambio de estado.
- Creación de actividad.
- Relación actividad-ticket.

El entorno local validado utiliza XAMPP/MariaDB en el puerto 3306. Docker Compose se conserva como alternativa arquitectónica existente, con MySQL publicado en el puerto 3310; no es requisito obligatorio para desarrollo local.

`database/001_tecnologia.sql` crea las tablas y EF Core las mapea. No se utiliza Code First para crear el esquema. La validación técnica no convierte el modelo del prototipo en el modelo funcional definitivo ni implica preparación para producción.

## Etapa 1 — Modelado funcional

Estado: `EN DEFINICIÓN`.

Solicitudes, trabajo técnico, relaciones entre ambos, bitácora, usuarios, perfiles, permisos y clientes. Las cardinalidades y reglas se acordarán progresivamente mediante GitHub Issues.

## Etapa 2 — Gestión de desarrollo

Estado: `DISEÑADO / PENDIENTE`.

Épicas, historias, bugs, tareas, tablero y trazabilidad. La presencia de conceptos o controles en el prototipo no implica que esta etapa esté terminada.

## Etapa 3 — QA

Estado: `DISEÑADO / PENDIENTE`.

Apoyo al aseguramiento de calidad y su trazabilidad. El flujo definitivo queda `EN DEFINICIÓN`; la etapa de tablero actual no es un módulo completo de pruebas.

## Etapa 4 — Soporte y portal del cliente

Estado: `DISEÑADO / PENDIENTE`.

Soporte/Operaciones de TI y experiencia del cliente para crear solicitudes, aportar información/evidencias, consultar avances, responder y conocer la resolución. Estados, permisos y notificaciones quedan `EN DEFINICIÓN`.

## Etapa 5 — Base de conocimiento

Estado: `DISEÑADO / PENDIENTE`.

Problemas conocidos, procedimientos y soluciones reutilizables.

## Etapa 6 — Integraciones

Estado: `VISIÓN FUTURA`.

Explorar posibles relaciones con GitHub Issues, Pull Requests, commits y branches, y la futura propuesta de integración con DrMax. Ninguna integración se presenta como decisión tomada; GitHub conserva su función de repositorio de código.

## Etapa 7 — Asistente virtual

Estado: `VISIÓN FUTURA`.

Consultas iniciales apoyadas en conocimiento/documentación y escalamiento a atención humana cuando no se resuelvan. No existe actualmente ni forma parte de lo implementado. Arquitectura, tecnología, modelo y proveedor no están seleccionados.
