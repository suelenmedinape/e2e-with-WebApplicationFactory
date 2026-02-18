using System.ComponentModel.DataAnnotations.Schema;

namespace rifa_csharp.Entities;

public class Participant
{
    public int Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone {  get; set; }
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}