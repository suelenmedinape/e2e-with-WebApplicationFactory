using rifa_csharp.Entities;
using rifa_csharp.Repositories;

namespace rifa_csharp.Interface;

public interface IUnitOfWork
{
    IGenericRepository<Raffle> RaffleRepository { get; }
    IGenericRepository<Ticket> TicketRepository { get; }
    IGenericRepository<Participant> ParticipantRepository { get; }
    IGenericRepository<Gallery> GalleryRepository { get; }
    
    Task CommitAsync();
}