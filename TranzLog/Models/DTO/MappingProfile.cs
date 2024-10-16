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
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<RegisterDTO, User>().ForMember(dest => dest.Password, opt => opt.Ignore());
        }
    }
}
