using Agenciapp.Common.Constants;
using Agenciapp.Common.Exceptions;
using Agenciapp.Common.Services;
using AgenciappHome.Logger;
using AgenciappHome.Models.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgenciappHome.Middlewares
{
    public class AppMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISupportLoggingClient _supportLoggingClient;
        private readonly IConfiguration _configuration;
        public AppMiddleware(RequestDelegate next, 
            ILoggerFactory loggerFactory, 
            ISupportLoggingClient supportLoggingClient, 
            IConfiguration configuration)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            _supportLoggingClient = supportLoggingClient;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext, IWorkContext workContext)
        {
            try
            {
                if (httpContext != null)
                {
                    if (httpContext.User != null)
                    {
                        workContext.UserIsAuthenticated = true;
                        workContext.Token = httpContext.User.FindFirst(AppClaim.Token)?.Value;
                        workContext.TokenExpirationUtc = httpContext.User.FindFirst(AppClaim.TokenExpirationUtc)?.Value;
                    }
                }

                await _next(httpContext);
            }
            catch(ApiClientException apiEx)
            {
                if(apiEx.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await httpContext.SignOutAsync();
                    httpContext.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                }
                else
                    throw;
            }
            catch(AppException appEx)
            {
                var data = new
                {
                    Request = await FormatRequest(httpContext.Request),
                    Response = appEx.ResponseData,
                    Exception = JsonConvert.SerializeObject(appEx)
                };

                _ = _supportLoggingClient.LogIndex(new Logger.Models.IndexRequest
                {
                    Id = string.Empty,
                    Index = _configuration["SupportLoggingIndexError"],
                    Document = data
                });

                await HandleExceptionAsync(httpContext, appEx);

            }
            catch (Exception ex)
            {
                var data = new
                {
                    Request = await FormatRequest(httpContext.Request),
                    Exception = JsonConvert.SerializeObject(ex)
                };

                _ = _supportLoggingClient.LogIndex(new Logger.Models.IndexRequest
                {
                    Id = string.Empty,
                    Index = _configuration["SupportLoggingIndexError"],
                    Document = data
                });

                throw;
            }
        }

        private Task HandleExceptionAsync(HttpContext context, AppException exception)
        {
            return context.Response.WriteAsync(JsonConvert.SerializeObject(exception.ResponseData));
        }

        private async Task<object> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var bodyAsText = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            var bodyAsObject = JsonConvert.DeserializeObject(bodyAsText);
            return new 
            {
                Scheme = request.Scheme,
                HostName = request.Host.Value,
                Path = request.Path,
                Headers = request.Headers.Select(h => new { header = h.Key, value = h.Value.ToArray() }).ToArray(),
                Body = bodyAsObject
            };
        }

        private async Task<object> FormatResponse(HttpResponse response)
        {
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);
            //...and copy it into a string
            string responseAsText = await new StreamReader(response.Body).ReadToEndAsync();
            var responseAsObject = JsonConvert.DeserializeObject(responseAsText);
            //We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);
            //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
            return new { StatusCode = response.StatusCode, Body = responseAsObject };
        }
    }
}
