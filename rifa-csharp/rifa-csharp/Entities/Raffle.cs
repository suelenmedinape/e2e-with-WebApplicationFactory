using rifa_csharp.Enums;

namespace rifa_csharp.Entities;

public class Raffle
{
    public int Id { get; set; }
    public string RaffleName { get; set; }
    public string Description { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public DateTime DrawDate  { get; set; }
    public RaffleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}