using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Task, TaskDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TaskTags.Select(tt => tt.Tag)));
        CreateMap<User, UserDto>();
        CreateMap<Tag, TagDto>();
    }
}
