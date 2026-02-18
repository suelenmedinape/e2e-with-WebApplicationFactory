namespace rifa_csharp.DTO.Participant;

public class ReadParticipantDTO
{
    public int Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone { get; set; }
    public List<int> TicketNumbers { get; set; }
}
