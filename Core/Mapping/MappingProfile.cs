using AutoMapper;
using Models;

namespace Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BoxCreateDto, Box>()
            .ReverseMap();
    }   
    
}