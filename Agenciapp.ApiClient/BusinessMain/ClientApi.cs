using Agenciapp.ApiClient.Models;
using Agenciapp.ApiClient.Utils;
using Agenciapp.Common.Models;
using Agenciapp.Common.Models.ClientModule;
using Agenciapp.Common.Models.Dto;
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
    public class ClientApi
    {
        private HttpClient Client { get; }
        private BusinessMainSetting _setting { get; set; }
        public ClientApi(HttpClient client, IOptions<BusinessMainSetting> options)
        {
            Client = client;
            _setting = options.Value;

            Client.BaseAddress = new Uri(_setting.BaseUrl);
            Client.Timeout = new TimeSpan(0, 1, 0);
            Client.DefaultRequestHeaders.Clear();
        }

        public ListFilterResponse<ClientDto> GetClientByfilter(Guid agencyId, ClientListQuery query)
        {
            Client.DefaultRequestHeaders.Add("AgencyId", agencyId.ToString());
            var result = Client.GetAsync($"{_setting.Client.GetByFilter}?{QueryUtils.GetQuery(query)}").Result;

            var stringResponse = result.Content.ReadAsStringAsync().Result; 
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<ClientDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }
    }
}
