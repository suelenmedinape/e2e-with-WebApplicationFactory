namespace rifa_csharp.DTO.Raffle;

public class ParticipantRaffleDTO
{
    public int Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone { get; set; }
    public List<int> TicketNumbers { get; set; }
}