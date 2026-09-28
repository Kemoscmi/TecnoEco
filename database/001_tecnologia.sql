-- Ejecutar únicamente sobre la base independiente tecnologia_db.
CREATE DATABASE IF NOT EXISTS tecnologia_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE tecnologia_db;
CREATE TABLE IF NOT EXISTS tecnologia_tickets (
 Id varchar(40) NOT NULL PRIMARY KEY, Title varchar(140) NOT NULL, Description text NOT NULL,
 Type varchar(40) NOT NULL, Origin varchar(40) NOT NULL, Priority varchar(40) NOT NULL,
 Requester varchar(160) NOT NULL, Assignee varchar(160) NOT NULL, Status varchar(40) NOT NULL,
 CreatedAt datetime(6) NOT NULL
);
CREATE TABLE IF NOT EXISTS tecnologia_actividades (
 Id varchar(40) NOT NULL PRIMARY KEY, TicketId varchar(40) NULL, Text text NOT NULL,
 Visibility varchar(40) NOT NULL, CreatedAt datetime(6) NOT NULL,
 INDEX IX_tecnologia_actividades_TicketId (TicketId),
 CONSTRAINT FK_actividades_tickets FOREIGN KEY (TicketId) REFERENCES tecnologia_tickets(Id) ON DELETE RESTRICT
);
