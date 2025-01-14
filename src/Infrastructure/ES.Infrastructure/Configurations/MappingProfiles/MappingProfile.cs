using AutoMapper;

using ES.Application.Dtos.Elevator;
using ES.Application.Dtos.Floor;
using ES.Domain.Entities;
using ES.Domain.Enums;


namespace ES.Infrastructure.Configurations.MappingProfiles;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Elevator, ElevatorInfo>()
            .ForMember(dest => dest.RequestQueue,
                opt => opt.MapFrom(src => new Queue<ElevatorRequest>(
                    src.RequestQueue.Select(id => new ElevatorRequest { Id = id }))))
            .ReverseMap();

        CreateMap<ElevatorInfo, Elevator>()
            .ForMember(dest => dest.RequestQueue,
                opt => opt.MapFrom(src => src.RequestQueue.Select(r => r.Id).ToList()));



        CreateMap<Floor, FloorInfo>()
            .ForMember(dest => dest.RequestQueue,
                opt => opt.MapFrom(src => new Queue<ElevatorRequest>(
                    src.RequestQueue.Select(id => new ElevatorRequest { Id = id }))))
            .ReverseMap();

        CreateMap<FloorInfo, Floor>()
            .ForMember(dest => dest.RequestQueue,
                opt => opt.MapFrom(src => src.RequestQueue.Select(r => r.Id).ToList()));


    }
}
