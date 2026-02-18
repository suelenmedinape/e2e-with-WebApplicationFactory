using rifa_csharp.Enums;

namespace rifa_csharp.DTO.Raffle;

public class CreateRaffleDTO
{
    public string RaffleName { get; set; }
    public string Description { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public DateTime DrawDate  { get; set; }
    public RaffleStatus Status { get; set; }
}