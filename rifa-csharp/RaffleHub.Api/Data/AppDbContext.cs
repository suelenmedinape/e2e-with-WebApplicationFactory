using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Data;

public class AppDbContext : DbContext
{
    public DbSet<Participant> Participant { get; set; }
    public DbSet<Raffle> Raffle { get; set; }
    public DbSet<Ticket> Ticket { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /*var adminRoleId = "8e4344d2-f611-4475-8e3d-275d3f25c7e1";
        var operatorRoleId = "8f3344d2-f611-4475-8e3d-275d3f25c7e2";
        var participantRoleId = "9a2344d2-f611-4475-8e3d-275d3f25c7e3";

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(
            new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = adminRoleId,
                Name = "ADMIN",
                NormalizedName = "ADMIN"
            },
            new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = operatorRoleId,
                Name = "OPERATOR",
                NormalizedName = "OPERATOR"
            },
            new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = participantRoleId,
                Name = "PARTICIPANT",
                NormalizedName = "PARTICIPANT"
            }
        );*/
        
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Raffle)
            .WithMany(r => r.Tickets)
            .HasForeignKey(t => t.RaffleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Participant)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ParticipantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Participant)
            .WithMany()
            .HasForeignKey(b => b.ParticipantId);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Raffle)
            .WithMany()
            .HasForeignKey(b => b.RaffleId);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.RaffleId, t.TicketNumber })
            .IsUnique();
    }
}