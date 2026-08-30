using AutoMapper;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Entities;

namespace OmniTrxnService.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.DebitCredit, opt => opt.MapFrom(src => src.DebitCredit.ToString()))
                .ForMember(dest => dest.Vendor, opt => opt.MapFrom(src => src.Vendor.ToString()));
        }
    }
}
