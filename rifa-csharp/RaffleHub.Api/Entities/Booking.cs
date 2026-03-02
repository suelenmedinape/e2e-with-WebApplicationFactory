using RaffleHub.Api.Enums;

namespace RaffleHub.Api.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PENDING;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    public Guid ParticipantId { get; set; }
    public virtual Participant Participant { get; set; }

    public Guid RaffleId { get; set; }
    public virtual Raffle Raffle { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}