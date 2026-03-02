namespace RaffleHub.Api.Entities;

public class Participant
{
    public Guid Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone { get; set; }
    public string Cpf { get; set; }
    
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}