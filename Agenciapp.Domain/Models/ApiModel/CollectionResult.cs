using System.Collections.Generic;

namespace RapidMultiservice.Models
{
    public class CollectionResult<T> where T : class
    {
        public bool Success { get; set; }
        public List<T> Objects { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}