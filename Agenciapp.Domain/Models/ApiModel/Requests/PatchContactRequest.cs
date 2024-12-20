namespace RapidMultiservice.Models.Requests
{
    public class PatchContactRequest
    {
        public string ContactNumber { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Ci { get; set; }
        public Address Addres { get; set; }
    }
}