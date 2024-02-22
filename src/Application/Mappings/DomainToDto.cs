using Application.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class DomainToDto : Profile
    {
        public DomainToDto() 
        {
            CreateMap<Archive, ArchiveDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
