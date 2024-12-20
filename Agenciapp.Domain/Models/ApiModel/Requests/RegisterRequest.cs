using System.ComponentModel.DataAnnotations;

namespace RapidMultiservice.Models.Requests
{
    public class RegisterRequest
    {
        [Required]public string PhoneNumber { get; set; }
        [Required]public string Password { get; set; }
        [Required]public string Name { get; set; }
        [Required]public string LastName { get; set; }
        [Required]public string Email { get; set; }
    }
}