using rifa_csharp.Enums;

namespace rifa_csharp.DTO.Raffle;

public class ListAllRafflesDTO
{
    public int Id { get; set; }
    public string RaffleName { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public RaffleStatus Status { get; set; }
    public int SoldTicketsCount { get; set; }
}