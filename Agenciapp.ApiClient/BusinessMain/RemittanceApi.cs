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
    public class RemittanceApi
    {
        private HttpClient Client { get; }
        private BusinessRemittanceSetting _setting { get; set; }
        public RemittanceApi(HttpClient client, IOptions<BusinessRemittanceSetting> options)
        {
            Client = client;
            _setting = options.Value;
            Client.BaseAddress = new Uri(_setting.BaseUrl);
            Client.Timeout = new TimeSpan(0, 1, 0);
            Client.DefaultRequestHeaders.Clear();
        }

        public ListFilterResponse<RemittanceDto> GetRemittanceByfilter(Guid agencyId, RemittanceListQuery query, string status)
        {
            query.Status = status;
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());
            HttpResponseMessage result = Client.GetAsync($"{_setting.GetRemittanceByFilter}?{QueryUtils.GetQuery(query)}").Result;
            var stringResponse = result.Content.ReadAsStringAsync().Result; 
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<RemittanceDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }
    }
}
