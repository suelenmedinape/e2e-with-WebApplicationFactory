using rifa_csharp.Data;
using rifa_csharp.Entities;
using rifa_csharp.Interface;

namespace rifa_csharp.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    private IGenericRepository<Raffle> raffleRepo;
    public IGenericRepository<Raffle> RaffleRepository
    {
        get { return raffleRepo ??= new GenericRepository<Raffle>(_context); }
    }
    
    private IGenericRepository<Ticket> ticketRepo;
    public IGenericRepository<Ticket> TicketRepository
    {
        get { return ticketRepo ??= new GenericRepository<Ticket>(_context); }
    }
    
    private IGenericRepository<Participant> participantRepo;
    public IGenericRepository<Participant> ParticipantRepository
    {
        get { return participantRepo ??= new GenericRepository<Participant>(_context); }
    }
    
    private IGenericRepository<Gallery> galleryRepo;
    public IGenericRepository<Gallery> GalleryRepository
    {
        get { return galleryRepo ??= new GenericRepository<Gallery>(_context); }
    }
    
    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }
}