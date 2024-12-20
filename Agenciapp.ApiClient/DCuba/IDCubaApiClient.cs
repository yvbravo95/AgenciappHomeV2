using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.ApiClient.DCuba
{
    public interface IDCubaApiClient
    {
        Task Emission(string token, Guid orderId);
        Task Update(string token, Guid orderId, string status);
    }

    public class DCubaApiClient: IDCubaApiClient
    {
        private readonly ILogger<DCubaApiClient> _logger;
        private readonly string _baseUrl;
        private readonly string _emission;
        private readonly string _update;
        public DCubaApiClient(IConfiguration configuration, ILogger<DCubaApiClient> logger)
        {
            _logger = logger;
            _baseUrl = configuration["DCubaApi:BaseUrl"];
            _emission = configuration["DCubaApi:Emission"];
            _update = configuration["DCubaApi:Update"];
        }

        public async Task Emission(string token, Guid orderId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("token", token);

                var httpResult = await client.PostAsync($"{_emission}/{orderId}", null).ConfigureAwait(false);
                string body = await httpResult.Content.ReadAsStringAsync();
                _logger.LogInformation(body);
            }
        }

        public async Task Update(string token, Guid orderId, string status)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("token", token);
                var data = new
                {
                    OrderId = orderId,
                    Status = status
                };

                var serialized = new StringContent( JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var httpResult = await client.PostAsync(_update, serialized).ConfigureAwait(false);
                string body = await httpResult.Content.ReadAsStringAsync();
                _logger.LogInformation(body);
            }
        }
    }
}
