using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class ChangePasswordRequest
    {
       
        [Required] public string OldPassword { get; set; }
        [Required] public string NewPassword { get; set; }
    }
}