using AgenciappHome.Logger.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgenciappHome.Logger
{
    public class SupportLoggingClient : ISupportLoggingClient
    {
        private readonly IConfiguration _configuration;
        public SupportLoggingClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LoggingBaseResponse> LogIndex(IndexRequest index)
        {
            try
            {
                var client = new RestClient(_configuration["SupportLoggingBaseUrl"]);
                var request = new RestRequest(_configuration["SupportLoggingEndpointIndex"], Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(JsonConvert.SerializeObject(index));

                var responseClient = await client.ExecuteAsync(request);
                if (!responseClient.IsSuccessful)
                {
                    return new LoggingBaseResponse
                    {
                        Action = responseClient.StatusCode,
                        Data = null,
                        Message = responseClient.Content
                    };
                }

                var response = JsonConvert.DeserializeObject<LoggingBaseResponse>(responseClient.Content);
                return response;
            }
            catch (Exception ex)
            {
                return new LoggingBaseResponse
                {
                    Action = System.Net.HttpStatusCode.InternalServerError,
                    Data = null,
                    Message = ex.Message
                };
            }
        }
    }
}
