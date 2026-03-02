using RaffleHub.Api.Enums;

namespace RaffleHub.Api.Entities;

public class Raffle
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } =
        "https://tcjbiwxyvozqnajhauro.supabase.co/storage/v1/object/sign/storage-iff-road/raffle/image_not_found.png?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjE2NzE2MC0zOGExLTQwMGMtYjkxZS0xZjg5YWFhYTc1ZGYiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJzdG9yYWdlLWlmZi1yb2FkL3JhZmZsZS9pbWFnZV9ub3RfZm91bmQucG5nIiwiaWF0IjoxNzcyNDU0MzEzLCJleHAiOjE3NzMzMTgzMTN9.S6L7dGlRCHWuJLrRHaN6a-Xy0P_9ShI9VPwyytgcSD0";
    public string FolderName { get; set; } = "raffle";
    public string RaffleName { get; set; }
    public string Description { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public DateTime DrawDate  { get; set; }
    public RaffleStatus Status { get; set; } = RaffleStatus.ACTIVE;
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}