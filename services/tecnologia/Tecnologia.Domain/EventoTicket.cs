namespace Tecnologia.Domain;

// Contrato preparatorio para REQ-011. No se genera, persiste ni consulta todavía.
// Activity conserva su carácter de actividad del prototipo.
public sealed record EventoTicket
{
    public string TicketId { get; }
    public string ActorId { get; }
    public DateTimeOffset FechaUtc { get; }
    public string TipoEvento { get; }

    public EventoTicket(string ticketId, string actorId, DateTimeOffset fechaUtc, string tipoEvento)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ticketId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoEvento);
        TicketId = ticketId;
        ActorId = actorId;
        FechaUtc = fechaUtc.ToUniversalTime();
        TipoEvento = tipoEvento;
    }
}
