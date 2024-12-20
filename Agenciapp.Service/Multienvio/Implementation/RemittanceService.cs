using Agenciapp.Common.Class;
using Agenciapp.Service.Multienvio.Interfaces;
using Agenciapp.Service.Multienvio.Models;
using CSharpFunctionalExtensions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;


namespace Agenciapp.Service.Multienvio.Implementation
{
    public class RemittanceService : IRemittanceService
    {
        private readonly GraphQLHttpClient _graphQl;
        public string AuthorizationId { get; set; }
        public RemittanceService(string baseUrl)
        {
            _graphQl = new GraphQLHttpClient(baseUrl, new NewtonsoftJsonSerializer());
        }

        public Result<string> Create(CreateRemittanceModel remittance)
        {
            if (string.IsNullOrEmpty(AuthorizationId))
                return Result.Failure<string>("Multienvio. El parametro authorization es requerido.");

            var query = new GraphQLHttpRequestWithHeadersSupport(AuthorizationId)
            {
                Query = @"
                mutation {
                createCashDeliveries(
                    item: [
                      {" +
                        $"municipalityName: \"{remittance.municipalityName}\"" +
                        $"provinceName: \"{remittance.provinceName}\"" +
                        $"agencyNote: \"{remittance.agencyNote}\"" + 
                        $"neighborhood: \"{remittance.neighborhood}\"" +
                        $"currency: {remittance.currency.GetDescription()} " + 
                        $"amount: {remittance.amount} " +
                        @"receiver:
                        {" +
                          $"name: \"{remittance.receiver.name}\"" +
                          $"lastName: \"{remittance.receiver.lastName}\"" +
                          $"address: \"{remittance.receiver.address}\"" +
                          $"landLinePhone: \"{remittance.receiver.landLinePhone}\"" +
                          $"phone: \"{remittance.receiver.phone}\"" +
                        @"}
                        deliveryServerData:
                        {" +
                          $"serverDate: \"{remittance.deliveryServerData.serverDate.ToString("yyyy-MM-dd HH:mm:ss")}\"" + 
                          $"id: \"{remittance.deliveryServerData.id}\"" +
                        @"}
                      }
                    ]
                  ) {
                    _id
                    }
                }
                "
            };
            var response = _graphQl.SendMutationAsync<RemittanceResult>(query).Result;
            if(response.Errors != null)
            {
                string errors = "Multienvio. ";
                foreach (var error in response.Errors)
                    errors += $"{error.Message},";
                return Result.Failure<string>(errors);
            }

            return Result.Success(response.Data.CreateCashDeliveries.First()._id);
        }

        public Result Edit(EditRemittanceModel remittance)
        {
            if (string.IsNullOrEmpty(AuthorizationId))
                return Result.Failure<string>("Multienvio. El parametro authorization es requerido.");

            var query = new GraphQLHttpRequestWithHeadersSupport(AuthorizationId)
            {
                Query = @"
                mutation {" +
                $"updateCashDelivery(id: \"{remittance.TransactionId}\", " + 
                    @"item: 
                      {" +
                        $"municipalityName: \"{remittance.municipalityName}\"" +
                        $"provinceName: \"{remittance.provinceName}\"" +
                        $"agencyNote: \"{remittance.agencyNote}\"" +
                        $"neighborhood: \"{remittance.neighborhood}\"" +
                        $"currency: {remittance.currency.GetDescription()} " +
                        $"amount: {remittance.amount} " +
                        @"receiver:
                        {" +
                          $"name: \"{remittance.receiver.name}\"" +
                          $"lastName: \"{remittance.receiver.lastName}\"" +
                          $"address: \"{remittance.receiver.address}\"" +
                          $"landLinePhone: \"{remittance.receiver.landLinePhone}\"" +
                          $"phone: \"{remittance.receiver.phone}\"" +
                        @"}
                        deliveryServerData:
                        {" +
                          $"serverDate: \"{remittance.deliveryServerData.serverDate.ToString("yyyy-MM-dd HH:mm:ss")}\"" +
                          $"id: \"{remittance.deliveryServerData.id}\"" +
                        @"}
                      }
                  ) {
                    _id
                    }
                }
                "
            };
            var response = _graphQl.SendMutationAsync<RemittanceResult>(query).Result;
            if (response.Errors != null)
            {
                string errors = "Multienvio. ";
                foreach (var error in response.Errors)
                    errors += $"{error.Message},";
                return Result.Failure<string>(errors);
            }

            return Result.Success();
        }

        public Result Cancel(string transactionId)
        {
            if (string.IsNullOrEmpty(AuthorizationId))
                return Result.Failure<string>("Multienvio. El parametro authorization es requerido.");

            var query = new GraphQLHttpRequestWithHeadersSupport(AuthorizationId)
            {
                Query = @"
                    mutation cancel{" +
                      $"cancelDelivery(id:\"{transactionId}\")" +
                      @"{
                       status
                       }
                    }
                "
            };
            var response = _graphQl.SendMutationAsync<RemittanceResult>(query).Result;
            if (response.Errors != null)
            {
                string errors = "Multienvio. ";
                foreach (var error in response.Errors)
                    errors += $"{error.Message},";
                return Result.Failure<string>(errors);
            }

            return Result.Success();
        }

        private class GraphQLHttpRequestWithHeadersSupport : GraphQLHttpRequest
        {
            private readonly string _authId;
            public GraphQLHttpRequestWithHeadersSupport(string authId)
            {
                _authId = authId;
            }
            public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
            {
                var r = base.ToHttpRequestMessage(options, serializer);
                r.Headers.Add("Authorization", _authId);
                return r;
            }
        }
    }
}