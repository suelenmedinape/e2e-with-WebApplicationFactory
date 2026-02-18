using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using rifa_csharp.Entities;

namespace rifa_csharp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Participant> Participant { get; set; }
    public DbSet<Raffle> Raffle { get; set; }
    public DbSet<Ticket> Ticket { get; set; }
    public DbSet<Gallery> Gallery { get; set; }
    public DbSet<User> User { get; set; }
    
    private readonly IConfiguration _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Raffle)
            .WithMany(r => r.Tickets)
            .HasForeignKey(t => t.RaffleId);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Participant)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ParticipantId)
            .IsRequired(false);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.RaffleId, t.TicketNumber })
            .IsUnique();
    }
}