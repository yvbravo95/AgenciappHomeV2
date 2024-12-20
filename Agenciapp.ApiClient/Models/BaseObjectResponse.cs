namespace Agenciapp.ApiClient.Models
{
    public class BaseObjectResponse<T>
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }
}
