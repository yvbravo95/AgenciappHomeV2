using Agenciapp.ApiClient.Models;
using Agenciapp.ApiClient.Utils;
using Agenciapp.Common.Models;
using Agenciapp.Common.Models.Dto;
using Agenciapp.Common.Models.PassportModule;
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
    public class PassportApi
    {
        private HttpClient Client { get; }
        private BusinessPassportSetting _setting { get; set; }
        public PassportApi(HttpClient client, IOptions<BusinessPassportSetting> options)
        {
            Client = client;
            _setting = options.Value;
            Client.BaseAddress = new Uri(_setting.BaseUrl);
            Client.Timeout = new TimeSpan(0, 1, 0);
            Client.DefaultRequestHeaders.Clear();
        }

        public ListFilterResponse<PassportDto> GetImportadosByfilter(Guid agencyId, PassportListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetImportadosByFilter}?{QueryUtils.GetQuery(query)}").Result;
            var stringResponse = result.Content.ReadAsStringAsync().Result; 
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<PassportDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ListFilterResponse<PassportDto> GetPassportByFilter(Guid agencyId, PassportListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());

            HttpResponseMessage result = Client.GetAsync($"{_setting.GetPassportByFilter}?{QueryUtils.GetQuery(query)}").Result; ;

            var stringResponse = result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<PassportDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }
    }
}
