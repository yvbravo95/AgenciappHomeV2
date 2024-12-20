using AgenciappHome.Models.Auxiliar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class CashAccountingBoxItem
    {
        public CashAccountingBoxItem()
        {
            recBancariaAgencies = new List<RecBancariaAgency>();
            CashAdjustments = new List<CashAdjustment>();
        }
        [Key]
        public Guid CashAccountingBoxItemId { get; set; }
        public string Number { get; set; }
        public Guid AgencyId { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateInit { get; set; }
        public DateTime DateEnd { get; set; }
        public List<RecBancariaAgency> recBancariaAgencies { get; set; }
        public List<CashAdjustment> CashAdjustments { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        public Guid? CashBoxId { get; set; }
        [ForeignKey("CashBoxId")]
        public CashBox CashBox { get; set; }
    }

    public class CashBox
    {
        public CashBox()
        {
            cashAccountingBoxItems = new List<CashAccountingBoxItem>();
            BoxHistories = new List<BoxHistory>();
        }
        [Key]
        public Guid CashBoxId { get; set; }
        public Guid AgencyId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        public List<CashAccountingBoxItem> cashAccountingBoxItems { get; set; }
        public List<BoxHistory> BoxHistories { get; set; }
    }

    public class CashAdjustment
    {
        [Key]
        public Guid CashAdjustmentId { get; set; }
        public string TypeAdjustment { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public Guid? cashAccountingBoxItemId { get; set; }
        [ForeignKey("cashAccountingBoxItemId")]
        public CashAccountingBoxItem cashAccountingBoxItem { get; set; }
    }

    public class BoxHistory
    {
        [Key] public Guid BoxHistoryId { get; set; }
        public DateTime Date { get; set; }
        public decimal Monto { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public Guid CashBoxId { get; set; }
        [ForeignKey("CashBoxId")] public CashBox CashBox { get; set; }
    }
}
