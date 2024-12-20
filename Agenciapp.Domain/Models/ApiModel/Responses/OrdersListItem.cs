using System;

namespace RapidMultiservice.Models.Responses
{
    public class OrdersListItem
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

       
    }
}