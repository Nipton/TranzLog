using AutoMapper;

namespace TranzLog.Models.DTO
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Shipper, ShipperDTO>().ReverseMap();
            CreateMap<Consignee, ConsigneeDTO>().ReverseMap();
            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<Cargo, CargoDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<UserDTO, User>().ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<RegisterDTO, User>().ForMember(dest => dest.Password, opt => opt.Ignore());
        }
    }
}
