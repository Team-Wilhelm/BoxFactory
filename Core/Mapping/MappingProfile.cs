using AutoMapper;
using Models.DTOs;
using Models.Models;

namespace Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BoxCreateDto, Box>()
            .ForMember(dest => dest.Dimensions,
                opt => opt
                    .MapFrom(src => new Dimensions
                    {
                        Length = src.DimensionsDto!.Length,
                        Width = src.DimensionsDto.Width,
                        Height = src.DimensionsDto.Height
                    }))
            .ReverseMap();

        CreateMap<BoxUpdateDto, Box>()
            .ForMember(dest => dest.Dimensions,
                opt => opt
                    .MapFrom(src => new Dimensions
                    {
                        Length = src.DimensionsDto!.Length,
                        Width = src.DimensionsDto.Width,
                        Height = src.DimensionsDto.Height
                    }))
            .ReverseMap();

        CreateMap<Dimensions, DimensionsDto>();
    }  
}