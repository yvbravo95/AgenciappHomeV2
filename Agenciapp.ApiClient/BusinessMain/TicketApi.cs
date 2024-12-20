using Agenciapp.ApiClient.Models;
using Agenciapp.ApiClient.Utils;
using Agenciapp.Common.Models;
using Agenciapp.Common.Models.Dto;
using Agenciapp.Common.Models.TicketModule;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Agenciapp.ApiClient.BusinessMain
{
    public class TicketApi
    {
        private HttpClient Client { get; }
        private BusinessTicketSetting _setting { get; set; }
        public TicketApi(HttpClient client, IOptions<BusinessTicketSetting> options)
        {
            Client = client;
            _setting = options.Value;
            Client.BaseAddress = new Uri(_setting.BaseUrl);
            Client.Timeout = new TimeSpan(0, 1, 0);
            Client.DefaultRequestHeaders.Clear();
        }

        public ListFilterResponse<TicketDto> GetMisOrdenesByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetMisOrdenesByFilter}?{QueryUtils.GetQuery(query)}").Result;
            var stringResponse = result.Content.ReadAsStringAsync().Result; 
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<TicketDto> GetPasajeByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetPasajeByFilter}?{QueryUtils.GetQuery(query)}").Result; ;

            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<TicketDto> GetAutoByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetAutoByFilter}?{QueryUtils.GetQuery(query)}").Result; ;

            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<TicketDto> GetHotelByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetHotelByFilter}?{QueryUtils.GetQuery(query)}").Result; ;

            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<TicketDto> GetPasajeCanceladasByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            var q_pasaje = QueryUtils.GetQuery(query);
            if (!string.IsNullOrEmpty(q_pasaje))
            {
                q_pasaje += "&canceladas=true";
            }
            else
            {
                q_pasaje = "canceladas=true";
            }

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetPasajeByFilter}?{q_pasaje}").Result; ;
            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<TicketDto> GetHotelCanceladasByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            var q_hotel = QueryUtils.GetQuery(query);
            if (!string.IsNullOrEmpty(q_hotel))
            {
                q_hotel += "&canceladas=true";
            }
            else
            {
                q_hotel = "canceladas=true";
            }

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetHotelByFilter}?{q_hotel}").Result; ;
            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<TicketDto> GetAutoCanceladasByfilter(Guid agencyId, TicketListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            var q_auto = QueryUtils.GetQuery(query);
            if (!string.IsNullOrEmpty(q_auto))
            {
                q_auto += "&canceladas=true";
            }
            else
            {
                q_auto = "canceladas=true";
            }

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetAutoByFilter}?{q_auto}").Result; ;
            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<TicketDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }
    }
}
