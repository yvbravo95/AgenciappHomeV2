using Agenciapp.ApiClient.Models;
using Agenciapp.Common.Exceptions;
using Agenciapp.Common.Headers;
using Agenciapp.Common.Models;
using Agenciapp.Common.Models.ClientModule;
using Agenciapp.Common.Models.Dto;
using Agenciapp.Common.Models.ShippingModule;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Agenciapp.ApiClient.BusinessMain
{
    public class ShippingApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private BusinessShippingSetting _setting { get; set; }
        public ShippingApi(IHttpClientFactory httpClientFactory, IOptions<BusinessShippingSetting> options)
        {
            _httpClientFactory = httpClientFactory;
            _setting = options.Value;
        }

        public ListFilterResponse<ShippingDto> GetShippingByfilter(Header header, ShippingListQuery query)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_setting.BaseUrl);

            client.DefaultRequestHeaders.Add(nameof(header.AgencyId), header.AgencyId.ToString());
            client.DefaultRequestHeaders.Add(nameof(header.UserId), header.UserId.ToString());
            var result = client.GetAsync($"{_setting.Shipping.GetByFilter}?{GetQuery(query)}").Result;

            var stringResponse = result.Content.ReadAsStringAsync().Result; 
            var response = JsonConvert.DeserializeObject<BaseObjectResponse<ListFilterResponse<ShippingDto>>>(stringResponse);
            if (!result.IsSuccessStatusCode)
                throw new Exception(response.Message);

            return response.Data;
        }

        public ShippingDto GetShippingById(Header header, Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_setting.BaseUrl);

                client.DefaultRequestHeaders.Add(nameof(header.AgencyId), header.AgencyId.ToString());
                client.DefaultRequestHeaders.Add(nameof(header.UserId), header.UserId.ToString());
                var result = client.GetAsync($"{_setting.Shipping.GetById}/{id}").Result;

                var stringResponse = result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<BaseObjectResponse<ShippingDto>>(stringResponse);
                if (!result.IsSuccessStatusCode)
                    throw new ApiClientException(result.StatusCode, response.Message);

                return response.Data;
            }
            catch (HttpRequestException e)
            {
                throw new ApiClientException(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        private string GetQuery<T>(T data)
        {
            var properties = from p in data.GetType().GetProperties()
                             where p.GetValue(data, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(data, null).ToString());

            string queryString = string.Join("&", properties.ToArray());
            return queryString;
        }
    }
}
