namespace rifa_csharp.Entities;

public class Ticket
{
    public int Id { get; set; }
    public int RaffleId { get; set; }
    public virtual Raffle Raffle { get; set; }
    public int? ParticipantId { get; set; }
    public virtual Participant? Participant { get; set; }
    public int TicketNumber { get; set; }
    public bool Sold { get; set; }
}