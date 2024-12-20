using System;

namespace AgenciappHome.Models.Templates.Store
{
    public class CreateStoreSetting
    {
        public Guid? Id { get; set; }
        public decimal FeeWholesaler { get; set; }
        public decimal FeeRetail { get; set; }
        public decimal ServiceCost { get; set; }
    }
}
