using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class ChangePhoneNumberRequest
    {
       
        [Required] public string NewPhoneNumber { get; set; }
        
    }
}