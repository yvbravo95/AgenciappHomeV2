using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class ConfirmResetPasswordRequest
    {
        [Required] public string SmsCode { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public string NewPassword { get; set; }
    }
}