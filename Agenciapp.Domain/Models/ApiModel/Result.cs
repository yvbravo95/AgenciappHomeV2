namespace RapidMultiservice.Models
{
    public class Result<T> where T : class
    {
        public bool Success { get; set; }
        public T Object { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}