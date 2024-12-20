using System.Net;

namespace Agenciapp.Common.Exceptions
{
    public class ApiClientException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public ApiClientException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
