namespace rifa_csharp.DTO.Participant;

public class UpdateParticipantDTO
{
    public int RaffleId { get; set; }
    public string ParticipantName { get; set; }
    public string Phone { get; set; }
    public List<int> TicketNumbers { get; set; }
}
