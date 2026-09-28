# Entorno local

Seguir README.md en la raíz. No configurar conexiones del sistema original.

Node 22 / npm para frontend, .NET 9 para backend, MySQL 8 para almacenamiento. Docker Compose opcional facilita MySQL y APIs.

La solución usa puertos diferentes al proyecto origen. El nuevo proyecto no tiene UserSecretsId heredados, credenciales, workflows Azure ni repositorio remoto heredado. El remoto propio actual es https://github.com/Kemoscmi/TecnoEco.git.


El entorno local validado de TecnoEco usa XAMPP/MariaDB en el puerto 3306; MySQL 8 mediante Docker Compose se conserva como alternativa en el puerto 3310 del host. Docker no es obligatorio. Crear las tablas con database/001_tecnologia.sql y configurar la conexión mediante variables de entorno locales, sin guardar credenciales en archivos versionados. EF Core mapea el esquema creado por SQL; no lo crea mediante Code First.

