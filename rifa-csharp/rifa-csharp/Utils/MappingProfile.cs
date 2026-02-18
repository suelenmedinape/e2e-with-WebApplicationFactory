using AutoMapper;
using rifa_csharp.DTO.Gallery;
using rifa_csharp.DTO.Participant;
using rifa_csharp.DTO.Raffle;
using rifa_csharp.Entities;

namespace rifa_csharp.Utils;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Raffle, RaffleNamesDTO>();
        CreateMap<CreateRaffleDTO, Raffle>();
        CreateMap<UpdateRaffleDTO, Raffle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<CreateParticipantDTO, Participant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Tickets, opt => opt.Ignore());
        CreateMap<ChangeRaffleStatusDTO, Raffle>();
        CreateMap<Raffle, RaffleDetailsDTO>();
        CreateMap<UpdateParticipantDTO, Participant>();
        CreateMap<Participant, ParticipantDetailsDTO>()
            .ForMember(
                dest => dest.Tickets,
                opt => opt.MapFrom(src => src.Tickets.Select(t => t.TicketNumber))
            );
        CreateMap<CreateImageDTO, Gallery>();
        CreateMap<Gallery, GalleryDataDTO>();
    }
}