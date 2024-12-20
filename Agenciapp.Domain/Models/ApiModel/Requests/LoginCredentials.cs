using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class LoginCredentials
    {
        [Required] public string PhoneNumber { get; set; }
        [Required] public string Password { get; set; }
        public string FcmToken { get; set; }
    }
}