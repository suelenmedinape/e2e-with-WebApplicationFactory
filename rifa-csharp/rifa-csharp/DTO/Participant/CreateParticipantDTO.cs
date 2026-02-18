using rifa_csharp.Entities;

namespace rifa_csharp.DTO.Participant;

public class CreateParticipantDTO
{
    public string ParticipantName { get; set; }
    public string Phone {  get; set; }
    
    public int RaffleId { get; set; }
    public List<int> TicketNumbers { get; set; } = new();
}