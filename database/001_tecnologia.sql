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
 CreatedAt datetime(6) NOT NULL
);
CREATE TABLE IF NOT EXISTS tecnologia_ticket_colaboradores (
 TicketId varchar(40) NOT NULL,
 ColaboradorId varchar(160) COLLATE utf8mb4_bin NOT NULL,
 PRIMARY KEY (TicketId, ColaboradorId),
 CONSTRAINT FK_colaboradores_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS tecnologia_actividades (
 Id varchar(40) NOT NULL PRIMARY KEY, TicketId varchar(40) NULL, Text text NOT NULL,
 Visibility varchar(40) NOT NULL, CreatedAt datetime(6) NOT NULL,
 INDEX IX_tecnologia_actividades_TicketId (TicketId),
 CONSTRAINT FK_actividades_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
