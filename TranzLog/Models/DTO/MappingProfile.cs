using AutoMapper;

namespace TranzLog.Models.DTO
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Shipper, ShipperDTO>().ReverseMap();
        }
    }
}
