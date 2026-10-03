
public class TicketUpdate
{
    public int Id {get; set;}
    public int TicketId {get; set;}
    public required Ticket Ticket {get; set;}
    public int UpdatedById {get; set;}
    public Person? UpdatedBy {get; set;}
    public DateTime UpdatedAt {get; set;}
    public string? Note {get; set;}

    private TicketUpdate() {}
    public TicketUpdate(Person updatedBy, Ticket ticket, string? note)
    {
        UpdatedBy = updatedBy;
        Ticket = ticket;
        TicketId = ticket.TicketId;
        Note = note;
        UpdatedAt = DateTime.UtcNow;
    }
}