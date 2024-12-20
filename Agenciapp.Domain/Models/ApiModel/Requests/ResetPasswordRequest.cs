using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class ResetPasswordRequest
    {
        [Required] public string PhoneNumber { get; set; }
    }
}