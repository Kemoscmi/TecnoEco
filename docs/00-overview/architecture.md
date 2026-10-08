# Arquitectura

Monorepo con SPA desacoplada → Gateway YARP → Tecnología.API → Application → Domain; Infrastructure implementa repositorios EF Core/MySQL.

El frontend conserva carpetas assets, components, plugins, router, services, stores, types y views. Reutiliza las versiones y el preset PrimeVue del proyecto de origen; el menú muestra únicamente Tecnología.

Los tickets representan solicitudes: categoría, estado, solicitante por ID, responsable opcional y colaboradores en una relación independiente. No incluyen prioridades. Consulta el [modelo REQ-001](../02-domains/REQ-001-modelo-base-solicitudes.md). Una actividad heredada puede estar vinculada a un ticket o ser general. El prototipo guarda cambios de estado y actividades juntos mediante SaveChanges de EF Core; esto no constituye la auditoría definitiva de REQ-011.

Modo demo: localStorage en el navegador. Modo API: MySQL independiente. Los modos no sincronizan datos. No existe autenticación implementada ni aislamiento entre clientes; solo uso de desarrollo local.

