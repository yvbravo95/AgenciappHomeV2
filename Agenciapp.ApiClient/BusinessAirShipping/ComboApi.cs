using Agenciapp.ApiClient.Models;
using Agenciapp.ApiClient.Utils;
using Agenciapp.Common.Headers;
using Agenciapp.Common.Models;
using Agenciapp.Common.Models.AirShippingModule;
using Agenciapp.Common.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.ApiClient.BusinessAirShipping
{
    public class ComboApi
    {
        private HttpClient _client { get; }
        private readonly BusinessAirShippingSetting _setting;
        public ComboApi(IOptions<BusinessAirShippingSetting> setting, HttpClient client)
        {
            _setting = setting.Value;
            _client = client;
            _client.BaseAddress = new Uri(_setting.BaseUrl);
            _client.Timeout = new TimeSpan(0, 1, 0);
            _client.DefaultRequestHeaders.Clear();
        }

        public async Task<ListFilterResponse<ComboListDto>> GetByFilter(string authToken, ComboListQuery query)
        {
            _client.DefaultRequestHeaders.Add("token", authToken);

            HttpResponseMessage result = await _client.GetAsync($"{_setting.GetCombosByFilter}?{QueryUtils.GetQuery(query)}");
            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<ComboListDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }
    }
}
