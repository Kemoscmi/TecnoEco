using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Tecnologia.Domain;
using Tecnologia.Infrastructure;

static class PersistenceChecks
{
    public static async Task Run()
    {
        var server = Environment.GetEnvironmentVariable("REQ001_TEST_SERVER");
        var connection = server ?? "Server=127.0.0.1;User ID=unused;Database=unused";
        var options = new DbContextOptionsBuilder<TechnologyDbContext>()
            .UseMySql(connection, new MariaDbServerVersion(new Version(10, 4, 32))).Options;
        using (var db = new TechnologyDbContext(options))
        {
            var ticket = db.Model.FindEntityType(typeof(Ticket))!;
            Check(!ticket.FindProperty(nameof(Ticket.SolicitanteId))!.IsNullable, "Solicitante requerido en EF");
            Check(ticket.FindProperty(nameof(Ticket.ResponsableId))!.IsNullable, "Responsable nullable en EF");
            Check(new[] { "Type", "Origin", "Priority", "Requester", "Assignee", "Status" }.All(p => ticket.FindProperty(p) is null), "Sin campos heredados en EF");
            var colaborador = db.Model.FindEntityType(typeof(TicketColaborador))!;
            Check(colaborador.FindPrimaryKey()!.Properties.Select(p => p.Name).SequenceEqual(new[] { "TicketId", "ColaboradorId" }), "Colaborador único por Ticket");
            Check(colaborador.GetForeignKeys().Single().PrincipalEntityType == ticket, "Relación de colaboradores con Ticket");
            Check(db.Model.FindEntityType(typeof(EventoTicket)) is null, "Contrato de auditoría sin persistencia");
        }
        if (string.IsNullOrWhiteSpace(server))
        {
            Console.WriteLine("OMITIDA integración SQL: configurar REQ001_TEST_SERVER para una instancia de pruebas con permiso CREATE DATABASE.");
            return;
        }

        // Base exclusiva y efímera; nunca usa ni modifica tecnologia_db.
        var database = "req001_test_" + Guid.NewGuid().ToString("N");
        var builder = new MySqlConnectionStringBuilder(server) { Database = "" };
        await using var admin = new MySqlConnection(builder.ConnectionString);
        await admin.OpenAsync();
        try
        {
            var sql = (await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "001_tecnologia.sql")))
                .Replace("tecnologia_db", database, StringComparison.Ordinal);
            await new MySqlCommand(sql, admin).ExecuteNonQueryAsync();
            await new MySqlCommand(sql, admin).ExecuteNonQueryAsync();
            builder.Database = database;
            var actualOptions = new DbContextOptionsBuilder<TechnologyDbContext>()
                .UseMySql(builder.ConnectionString, new MariaDbServerVersion(new Version(10, 4, 32))).Options;
            string id;
            await using (var db = new TechnologyDbContext(actualOptions))
            {
                var ticket = new Ticket { Title = "Solicitud SQL", Description = "Prueba REQ-001", SolicitanteId = "externo:Aa-01" };
                id = ticket.Id;
                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
            }
            await using (var db = new TechnologyDbContext(actualOptions))
            {
                var repo = new TechnologyRepository(db);
                var ticket = (await repo.Find(id, default))!;
                Check(ticket.ResponsableId is null && ticket.Colaboradores.Count == 0 && ticket.SolicitanteId == "externo:Aa-01", "SQL conserva identidad, responsable null y cero colaboradores");
                ticket.ResponsableId = "soporte-01";
                ticket.Colaboradores.Add(new() { TicketId = id, ColaboradorId = "Colaborador-A" });
                ticket.Colaboradores.Add(new() { TicketId = id, ColaboradorId = "colaborador-a" });
                await repo.Save(default);
            }
            await using (var db = new TechnologyDbContext(actualOptions))
            {
                var repo = new TechnologyRepository(db);
                var ticket = (await repo.Tickets(default)).Single();
                Check(ticket.Colaboradores.Count == 2 && ticket.ResponsableId == "soporte-01", "Repositorio carga varios colaboradores e IDs sensibles a mayúsculas");
                Check(ticket.Estado == EstadosTicket.Recibida && ticket.Categoria == CategoriasTicket.Problema, "SQL conserva categoría y estado");
                db.Set<TicketColaborador>().Add(new() { TicketId = id, ColaboradorId = "Colaborador-A" });
                await RejectSave(db, "SQL rechaza colaborador duplicado");
            }
            await using (var db = new TechnologyDbContext(actualOptions))
            {
                db.Set<TicketColaborador>().Add(new() { TicketId = "inexistente", ColaboradorId = "externo-01" });
                await RejectSave(db, "SQL rechaza colaborador sin Ticket");
            }
        }
        finally
        {
            await new MySqlCommand($"DROP DATABASE IF EXISTS `{database}`", admin).ExecuteNonQueryAsync();
        }
    }

    static async Task RejectSave(TechnologyDbContext db, string message)
    {
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException e) when (e.InnerException is MySqlException { Number: 1062 or 1452 })
        { Console.WriteLine("OK: " + message); return; }
        throw new Exception(message);
    }

    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("OK: " + message);
    }
}
