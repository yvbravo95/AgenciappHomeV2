using System;
namespace RapidMultiservice.Models.Requests
{
    public class AccountDataPatchRequest
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Address Address { get; set; }
    }
}