using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.IO;
using Agenciapp.Service.HttpServices.Models;
using System.Text.Json;

namespace Agenciapp.Service.HttpServices
{
    public interface IHttpService
    {
        Task<ApiResponse> GetAsync(string requestUri);
        Task<ApiResponse> PostAsync(string requestUri, object data);
        Task<ApiResponse> PutAsync(string requestUri, object data);
        Task<ApiResponse> UploadAsync(string requestUri, object data);
        Task<ApiResponse> DeleteAsync(string requestUri);

        Task<ApiResponse<T>> GetAsync<T>(string requestUri);
        Task<ApiResponse<T>> PostAsync<T>(string requestUri, object data);
        Task<ApiResponse<T>> PutAsync<T>(string requestUri, object data);
        Task<ApiResponse<T>> UploadAsync<T>(string requestUri, string fileName, byte[] content);
        Task<ApiResponse<T>> DeleteAsync<T>(string requestUri);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private JsonSerializerOptions defaultJsonSerializerOptions =>
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        public HttpService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse> GetAsync(string requestUri)
        {
            return await ExecuteAsync(requestUri, "GET");
        }

        public async Task<ApiResponse> PostAsync(string requestUri, object data)
        {
            return await ExecuteAsync(requestUri, "POST", data);
        }

        public async Task<ApiResponse> PutAsync(string requestUri, object data)
        {
            return await ExecuteAsync(requestUri, "PUT", data);
        }

        public async Task<ApiResponse> UploadAsync(string requestUri, object data)
        {
            return await ExecuteAsync(requestUri, "UPLOAD", data);
        }

        public async Task<ApiResponse> DeleteAsync(string requestUri)
        {
            return await ExecuteAsync(requestUri, "DELETE");
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string requestUri)
        {
            return await ExecuteAsync<T>(requestUri, "GET");
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string requestUri, object data)
        {
            return await ExecuteAsync<T>(requestUri, "POST", data);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string requestUri, object data)
        {
            return await ExecuteAsync<T>(requestUri, "PUT", data);
        }

        public async Task<ApiResponse<T>> UploadAsync<T>(string requestUri, string fileName, byte[] content)
        {
            var multiContent = new MultipartFormDataContent();
            multiContent.Add(new ByteArrayContent(content), "file", fileName);

            return await ExecuteAsync<T>(requestUri, "UPLOAD", multiContent);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string requestUri)
        {
            return await ExecuteAsync<T>(requestUri, "DELETE");
        }

        private async Task<ApiResponse> ExecuteAsync(string requestUri, string httpMethod, object data = null)
        {
            try
            {
                var httpResponseMessage = await GetHttpResponseMessageAsync(requestUri, httpMethod, data);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await Deserialize<ApiResponse>(httpResponseMessage, defaultJsonSerializerOptions);
                }
                else
                {
                    var response = new ApiResponse();

                    response.SetError(httpResponseMessage.ReasonPhrase, await httpResponseMessage.Content.ReadAsStringAsync());

                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = new ApiResponse();

                response.SetError(ex);

                return response;
            }
        }

        private async Task<ApiResponse<T>> ExecuteAsync<T>(string requestUri, string httpMethod, object data = null)
        {
            try
            {
                var httpResponseMessage = await GetHttpResponseMessageAsync(requestUri, httpMethod, data);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await Deserialize<ApiResponse<T>>(httpResponseMessage, defaultJsonSerializerOptions);
                }
                else
                {
                    var response = new ApiResponse<T>();

                    response.SetError(httpResponseMessage.ReasonPhrase, await httpResponseMessage.Content.ReadAsStringAsync());

                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<T>();

                response.SetError(ex);

                return response;
            }
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessageAsync(string requestUri, string httpMethod, object data = null)
        {
            string dataJson = null;
            if (data != null && httpMethod != "UPLOAD")
            {
                dataJson = JsonSerializer.Serialize(data);
            }

            HttpResponseMessage httpResponseMessage;

            switch (httpMethod)
            {
                case "GET":
                    httpResponseMessage = await _httpClient.GetAsync(requestUri);
                    break;
                case "POST":
                    httpResponseMessage = await _httpClient.PostAsync(requestUri, new StringContent(dataJson, Encoding.UTF8, "application/json"));
                    break;
                case "PUT":
                    httpResponseMessage = await _httpClient.PutAsync(requestUri, new StringContent(dataJson, Encoding.UTF8, "application/json"));
                    break;
                case "UPLOAD":
                    httpResponseMessage = await _httpClient.PostAsync(requestUri, (HttpContent)data);
                    break;
                case "DELETE":
                    httpResponseMessage = await _httpClient.DeleteAsync(requestUri);
                    break;
                default:
                    throw new NotImplementedException(string.Format("Utility code not implemented for this Http method {0}", httpMethod));
            }

            return httpResponseMessage;
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage httpResponse, JsonSerializerOptions options)
        {
            var responseString = await httpResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseString))
            {
                return JsonSerializer.Deserialize<T>(responseString, options);
            }

            return default;
        }
    }
}
