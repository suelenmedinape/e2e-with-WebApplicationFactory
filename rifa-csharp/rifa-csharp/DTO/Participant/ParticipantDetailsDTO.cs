namespace rifa_csharp.DTO.Participant;

public class ParticipantDetailsDTO
{
    public int Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone {  get; set; }
    public List<int> Tickets { get; set; }
}