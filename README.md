# TecnoEco — Tecnología Ecosystem

Proyecto independiente basado en la arquitectura y el diseño de Optica Ecosystem. Único dominio: Tecnología.

## Inicio rápido — demostración

Requisitos: Node.js 22 y npm. Desde frontends/ecosystem:

```powershell
npm.cmd ci
npm.cmd run dev
```

Abrir http://localhost:5190. No requiere los servicios del proyecto original. Por defecto usa datos de ejemplo guardados en localStorage de este navegador y este origen; no se comparten entre equipos. Las respuestas clasificadas como públicas no se envían a nadie.

Incluye creación de tickets internos o de clientes, bugs, soporte, historias de usuario, tareas técnicas, filtros, tablero, cambios de estado, responsable, solicitante, notas y bitácora general. La bitácora registra automáticamente creación y cambios de estado. En QA es una etapa del tablero, todavía no un módulo de pruebas.

## Arquitectura

- frontends/ecosystem: Vue 3, TypeScript, Vite, Pinia, Vue Router, PrimeVue Aura (preset original).
- services/tecnologia: Tecnologia.API, Application, Domain e Infrastructure (.NET 9, EF Core, MySQL).
- gateway/TecnologiaGateway: YARP.
- shared/Tecnologia.Shared: contratos comunes independientes.
- database: SQL idempotente de la base tecnologia_db.
- docs: arquitectura, alcance y desarrollo.

## API con persistencia MySQL

Alternativa con Docker Compose (opcional para desarrollo local). Copiar .env.example a .env en la raíz y completar las dos contraseñas nuevas. Ejecutar docker compose up --build -d. La base se inicializa automáticamente solo cuando el volumen está vacío.

Copiar frontends/ecosystem/.env.example a .env.local en esa carpeta, cambiar VITE_DATA_MODE=api y reiniciar Vite. En modo API no se usan ni se importan los datos demo. Si la API falla se muestra el error, sin cambiar silenciosamente a modo demo.

Puertos exclusivos de este proyecto: frontend 5190, gateway 5100, API 5105, MySQL 3310. Volumen y red de Docker independientes. No se copiaron credenciales, datos, historial Git ni despliegues del sistema original.

Para ejecutar .NET sin Docker: crear la base con database/001_tecnologia.sql, configurar ConnectionStrings__TecnologiaDB en el entorno, ejecutar dotnet run --project services/tecnologia/Tecnologia.API y, en otra terminal, dotnet run --project gateway/TecnologiaGateway.

## Verificación

- Frontend: npm.cmd run build dentro de frontends/ecosystem.
- Backend: dotnet build tecnologia-ecosystem.sln.
- Reglas de negocio: dotnet run --project services/tecnologia/Tecnologia.Tests.

## Alcance actual

Base funcional de desarrollo, no lista para producción. El frontend es la vista del equipo interno. Pendientes: autenticación propia, roles y separación por cliente, identidad del autor en bitácora, portal de clientes, adjuntos, notificaciones y casos/evidencias QA. La visibilidad de notas es una clasificación, no un permiso de acceso implementado. API y gateway admiten solo entorno Development y Docker publica los puertos únicamente en loopback.

No se incluyeron módulos de óptica ni servicios auth originales dependientes de su organización y bases. En la creación del prototipo no se configuró un remoto Git ni se publicó nada. Ahora origin apunta a https://github.com/Kemoscmi/TecnoEco.git; esta preparación no publica commits.


## Visión y documentación de TecnoEco

Prototipo independiente para diseñar y validar un futuro Módulo de Tecnología que podría integrarse con DrMax. Consulta el [índice documental](docs/README.md) y la [visión y alcance](docs/00-producto/vision-y-alcance.md). GitHub Issues será el backlog vivo; docs consolidará lo acordado.

El entorno local validado utiliza XAMPP/MariaDB en el puerto 3306. Docker es una alternativa, no un requisito. El script database/001_tecnologia.sql crea las tablas y EF Core las mapea; no se usa Code First para crear el esquema.

