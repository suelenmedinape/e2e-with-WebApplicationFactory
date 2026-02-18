using AutoMapper;
using rifa_csharp.Repositories;

namespace rifa_csharp.Service;

public class UnitOfService
{
    private readonly UnitOfWork unit;
    private readonly IMapper mapper;
    private readonly SupabaseService supabaseService;
    
    private RaffleService raffleService;
    private ParticipantService participantService;
    private TicketService ticketService;
    private GalleryService galleryService;
    
    public UnitOfService(UnitOfWork unit, IMapper mapper, SupabaseService supabaseService)
    {
        this.unit = unit;
        this.mapper = mapper;
        this.supabaseService = supabaseService;
    }

    public RaffleService RaffleService => raffleService ??= new RaffleService(unit, mapper);
    public ParticipantService ParticipantService => participantService ??= new ParticipantService(unit, mapper);
    public TicketService TicketService => ticketService ??= new TicketService(unit, mapper);
    public GalleryService GalleryService => galleryService ??= new GalleryService(unit, mapper, supabaseService);
    public SupabaseService SupabaseService => supabaseService;
}