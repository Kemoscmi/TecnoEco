# Arquitectura

Monorepo con SPA desacoplada → Gateway YARP → Tecnología.API → Application → Domain; Infrastructure implementa repositorios EF Core/MySQL.

El frontend conserva carpetas assets, components, plugins, router, services, stores, types y views. Reutiliza las versiones y el preset PrimeVue del proyecto de origen; el menú muestra únicamente Tecnología.

Los tickets registran origen, tipo, prioridad, solicitante, responsable y estado. Una actividad puede estar vinculada a un ticket o ser general. Los cambios de estado y su entrada de bitácora se guardan juntos mediante SaveChanges de EF Core.

Modo demo: localStorage en el navegador. Modo API: MySQL independiente. Los modos no sincronizan datos. No existe autenticación implementada ni aislamiento entre clientes; solo uso de desarrollo local.

