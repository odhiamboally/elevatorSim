using AutoMapper;

using ES.Application.Dtos.Elevator;
using ES.Application.Dtos.Floor;
using ES.Domain.Entities;


namespace ES.Infrastructure.Configurations.MappingProfiles;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Elevator, ElevatorInfo>().ReverseMap();
        CreateMap<Floor, FloorInfo>().ReverseMap();

    }
}
