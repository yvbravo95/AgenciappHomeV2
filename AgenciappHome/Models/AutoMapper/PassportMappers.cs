using AgenciappHome.Models.Response;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.AutoMapper
{
    public class PassportMappers : Profile
    {
        public PassportMappers()
        {
            CreateMap<Passport, PassportStatusModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z.PassportId))
                .ForMember(x => x.Number, y => y.MapFrom(z => z.OrderNumber))
                .ForMember(x => x.Status, y => y.MapFrom(z => z.Status));
        }
    }
}
