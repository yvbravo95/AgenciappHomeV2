using Agenciapp.ApiClient.Models;
using Agenciapp.Common.Exceptions;
using Agenciapp.Common.Models.Security;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Agenciapp.ApiClient.Security
{

    public class SecurityLoginApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private SecuritySetting _setting { get; set; }
        public SecurityLoginApi(IHttpClientFactory httpClientFactory, IOptions<SecuritySetting> options)
        {
            _httpClientFactory = httpClientFactory;
            _setting = options.Value;
        }

        public AuthenticateResponse Authenticate(AuthenticateModel request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_setting.BaseUrl);

                var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var result = client.PostAsync($"{_setting.Login.Authenticate}", stringContent).Result;

                var stringResponse = result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<BaseObjectResponse<AuthenticateResponse>>(stringResponse);
                if (!result.IsSuccessStatusCode)
                    throw new ApiClientException(result.StatusCode, response?.Message);

                return response.Data;
            }
            catch(Exception e)
            {
                throw new ApiClientException(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
