using Agenciapp.ApiClient.Models;
using Agenciapp.ApiClient.Utils;
using Agenciapp.Common.Exceptions;
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
    public class OrderApi
    {
        private HttpClient _client { get; }
        private readonly BusinessAirShippingSetting _setting;
        public OrderApi(IOptions<BusinessAirShippingSetting> setting, HttpClient client)
        {
            _setting = setting.Value;

            _client = client;
            _client.BaseAddress = new Uri(_setting.BaseUrl);
            _client.Timeout = new TimeSpan(0, 1, 0);
            _client.DefaultRequestHeaders.Clear();
        }

        public ListFilterResponse<OrderListDto> GetByFilter(string authToken, OrderListQuery query)
        {
            try
            {
                _client.DefaultRequestHeaders.Add("Token", authToken);

                HttpResponseMessage result = _client.GetAsync($"{_setting.GetOrderByFilter}?{QueryUtils.GetQuery(query)}").Result;
                var stringResponse = result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<OrderListDto>>>(stringResponse);
                if (!result.IsSuccessStatusCode)
                    throw new ApiClientException(result.StatusCode, response.Message);

                return response.Data;
            }
            catch(HttpRequestException httpEx)
            {
                throw new ApiClientException(System.Net.HttpStatusCode.InternalServerError, httpEx.Message);
            }
        }
    }
}
