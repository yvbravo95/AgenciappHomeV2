using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class ConfirmPhoneNumberRequest
    {
        [Required]  public string SmsCode { get; set; }
    }
}