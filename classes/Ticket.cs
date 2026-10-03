
using System.ComponentModel.DataAnnotations.Schema;

[Table("Ticket", Schema ="dbo")]
public class Ticket
{
    [Column("id")]
    public int TicketId {get; private set;}
    [Column("priority")]
    public TicketPriority Priority {get; set;}
    [Column("status")]
    public TicketStatus Status {get; set;}
    [Column("type")]
    public TicketType Type {get; set;}
    public int CreatedById {get; private set;}
    public Person CreatedBy {get; private set;}
    [Column("notes")]
    public string TicketDetails {get; private set;}
    public int? UpdatedById {get; private set;}
    public Person? UpdatedBy {get;  set;}

    private Ticket() {}
    public Ticket(TicketType type, Person user, string details)
    {
        Status = TicketStatus.Submitted;
        TicketDetails = details;
        Type = type;
        CreatedBy = user;
        if (type == TicketType.Bug) {
            Priority = TicketPriority.High;
        } else if (type == TicketType.Feature)
        {
            Priority = TicketPriority.Low;
        } else
        {
            Priority = TicketPriority.Medium;
        }
    }
    public void UpdatePriority(TicketPriority priority)
    {
        Priority = priority;
    }
    public void UpdateStatus(TicketStatus status, Person admin)
    {
        Status = status;
        UpdatedBy = admin;
    }
}