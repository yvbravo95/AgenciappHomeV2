using Agenciapp.ApiClient.FlightProviders.Enums;
using Agenciapp.ApiClient.FlightProviders.Models;
using Agenciapp.Domain.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.ApiClient.FlightProviders
{
    public class FlightProvidersApi
    {
        private HttpClient Client { get; }
        private readonly string _apiBaseUrl;
        public FlightProvidersApi(HttpClient client, IConfiguration configuration)
        {
            _apiBaseUrl = configuration["FlightProvidersApi:BaseUrl"];
            if(string.IsNullOrEmpty(_apiBaseUrl))
            {
                throw new ArgumentNullException("FlightProvidersApi:BaseUrl is not configured properly.");
            }

            Client = client;
            Client.BaseAddress = new Uri(_apiBaseUrl);
            Client.Timeout = new TimeSpan(0, 5, 0);
            Client.DefaultRequestHeaders.Clear();

        }

        public async Task<ScrapeResponse> GetFlights(GetFlightRequest request, SearchType type)
        {
            int requestType = Convert.ToInt32(type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights?type={requestType}", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }

        public async Task<ScrapeResponse> GetCascuite(GetFlightRequest request)
        {
            int requestType = Convert.ToInt32(request.Type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights/cacsuite", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }

        public async Task<ScrapeResponse> GetHavanaair(GetFlightRequest request)
        {
            int requestType = Convert.ToInt32(request.Type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights/havanaair", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }

        public async Task<ScrapeResponse> GetInvictaair(GetFlightRequest request)
        {
            int requestType = Convert.ToInt32(request.Type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights/invictaair", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }

        public async Task<ScrapeResponse> GetEasypax(GetFlightRequest request)
        {
            int requestType = Convert.ToInt32(request.Type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights/easypax", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }

        public async Task<ScrapeResponse> Getxaelsuite(GetFlightRequest request)
        {
            int requestType = Convert.ToInt32(request.Type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights/xaelsuite", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }
        
        public async Task<ScrapeResponse> GetGflight(GetFlightRequest request)
        {
            int requestType = Convert.ToInt32(request.Type);

            HttpContent httpContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                origin = request.Origin,
                destination = request.Destination,
                departureDate = request.DepartureDate,
                returnDate = request.ReturnDate,
                passengers = new
                {
                    adults = request.Adults,
                    children = request.Children,
                    infants = request.Infants
                }
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PostAsync($"/flights/gflight", httpContent);

            var stringResponse = await result.Content.ReadAsStringAsync();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapeResponse>(stringResponse);

            if (!result.IsSuccessStatusCode)
                throw new Exception(response.ToString());

            return response;
        }
    }
}
