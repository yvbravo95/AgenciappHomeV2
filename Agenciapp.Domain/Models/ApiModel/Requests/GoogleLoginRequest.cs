using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class GoogleLoginRequest
    {
        [Required]public string GoogleToken { get; set; }
       
    }
}