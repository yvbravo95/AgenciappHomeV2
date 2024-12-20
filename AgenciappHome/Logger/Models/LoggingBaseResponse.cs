using System.Net;

namespace AgenciappHome.Logger.Models
{
    public class LoggingBaseResponse
    {
        public HttpStatusCode Action { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }
}
