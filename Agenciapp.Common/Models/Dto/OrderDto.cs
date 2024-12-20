using System;
using System.Collections.Generic;

namespace Agenciapp.Common.Models.Dto
{
    public class OrderDto
    {
        public OrderDto()
        {
            Bag = new List<BagDto>();
        }
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliverDate { get; set; }

        public string Number { get; set; }
        public string Status { get; set; }

        public bool Express { get; set; }
        public bool LackSend { get; set; }
        public bool LackReview { get; set; }

        public decimal WholesalerCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }

        public ClientDto Client { get; set; }
        public ContactDto Contact { get; set; }
        public AgencyDto Agency { get; set; }
        public AgencyDto AgencyTransferida { get; set; }
        public WholesalerDto Wholesaler { get; set; }
        public PackageDto Package { get; set; }
        public UserDto PrincipalDistributor { get; set; }
        public List<BagDto> Bag { get; set; }
    }
}
