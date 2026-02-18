namespace rifa_csharp.DTO.Raffle;

public class RaffleSummaryDto
{
    public string RaffleName { get; set; } = string.Empty;
    public int ParticipantsCount { get; set; }
    public int SoldTicketsCount { get; set; }
    public int TotalTicketsCount { get; set; }
}