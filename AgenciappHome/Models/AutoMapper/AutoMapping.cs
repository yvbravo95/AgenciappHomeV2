using AgenciappHome.Models.Auxiliar;
using AgenciappHome.Models.Response;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.AutoMapper
{
    public class AutoMapping:Profile
    {
        public AutoMapping()
        {
            CreateMap<Order, AuxModelTienda>() //Para el index de tienda
                .ForMember(dest =>
                dest.ClientFullName,
                opt => opt.MapFrom(src => src.Client.FullData))
                .ForMember(dest =>
                dest.AgencyTransferidaId,
                opt => opt.MapFrom(src => src.agencyTransferida.AgencyId))
                .ForMember(dest =>
                dest.AgencyName,
                opt => opt.MapFrom(src => src.Agency.Name))
                .ForMember(dest =>
                dest.AgencyTransferidaName,
                opt => opt.MapFrom(src => src.agencyTransferida.Name));

            CreateMap<GuiaAerea, GuiaAereaDto>();
        }
    }
}
