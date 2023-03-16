using AutoMapper;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Models;

namespace Task_2EF.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegistrationModel, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(x => x.Email));
        }
    }
}
