using AutoMapper;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Utils.Mappings;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Raffle, ListAllRaffleDto>()
            .ForMember(dest => dest.SoldTicketsCount, 
                opt => opt.MapFrom(src => src.Tickets.Count(t => t.ParticipantId != null)));
        
        CreateMap<CreateRaffleDto, Raffle>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Tickets, opt => opt.Ignore())
            .ForMember(dest => dest.ImageUrl, opt => opt.Condition(src => src.ImageUrl != null));
        
        CreateMap<UpdateRaffleDto, Raffle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Tickets, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}