-- Ejecutar únicamente sobre la base independiente tecnologia_db.
-- Inicializa una base vacía; IF NOT EXISTS no convierte tablas del prototipo anterior.
-- Véase docs/02-domains/REQ-001-modelo-base-solicitudes.md antes de reutilizar una base.
CREATE DATABASE IF NOT EXISTS tecnologia_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE tecnologia_db;
CREATE TABLE IF NOT EXISTS tecnologia_tickets (
 Id varchar(40) NOT NULL PRIMARY KEY, Title varchar(140) NOT NULL, Description text NOT NULL,
 Categoria varchar(40) NOT NULL,
 SolicitanteId varchar(160) COLLATE utf8mb4_bin NOT NULL,
 ResponsableId varchar(160) COLLATE utf8mb4_bin NULL,
 Estado varchar(40) NOT NULL,
 AbandonoRecibidaAt datetime(6) NULL,
 EliminadoAt datetime(6) NULL,
 EliminadoPorId varchar(160) COLLATE utf8mb4_bin NULL,
 CreatedAt datetime(6) NOT NULL
);
CREATE TABLE IF NOT EXISTS tecnologia_ticket_colaboradores (
 TicketId varchar(40) NOT NULL,
 ColaboradorId varchar(160) COLLATE utf8mb4_bin NOT NULL,
 PRIMARY KEY (TicketId, ColaboradorId),
 CONSTRAINT FK_colaboradores_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS tecnologia_ticket_trabajos_tecnicos (
 TicketId varchar(40) NOT NULL,
 TrabajoTecnicoId varchar(160) COLLATE utf8mb4_bin NOT NULL,
 TipoTrabajoTecnico varchar(40) NOT NULL,
 RelacionadoPorId varchar(160) COLLATE utf8mb4_bin NOT NULL,
 CreatedAt datetime(6) NOT NULL,
 PRIMARY KEY (TicketId, TrabajoTecnicoId),
 CONSTRAINT FK_trabajos_tecnicos_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
-- REQ-015: registro de notificaciones. Canal (interno/correo) por definir.
CREATE TABLE IF NOT EXISTS tecnologia_notificaciones (
 Id varchar(40) NOT NULL PRIMARY KEY,
 DestinatarioId varchar(160) COLLATE utf8mb4_bin NOT NULL,
 TicketId varchar(40) NOT NULL,
 TipoNotificacion varchar(60) NOT NULL,
 Mensaje text NOT NULL,
 Leida tinyint(1) NOT NULL DEFAULT 0,
 FechaUtc datetime(6) NOT NULL,
 INDEX IX_notificaciones_destinatario (DestinatarioId),
 CONSTRAINT FK_notificaciones_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS tecnologia_actividades (
 Id varchar(40) NOT NULL PRIMARY KEY, TicketId varchar(40) NULL, Text text NOT NULL,
 Visibility varchar(40) NOT NULL, CreatedAt datetime(6) NOT NULL,
 INDEX IX_tecnologia_actividades_TicketId (TicketId),
 CONSTRAINT FK_actividades_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
