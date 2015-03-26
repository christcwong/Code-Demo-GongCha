using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class TransferOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int ParentOrderID { get; set; }
        public virtual TransferOrder ParentOrder { get; set; }

        public int ItemId { get; set; }
        public virtual Item Item { get; set; }

        [Display(Name="Quantity To Destination")]
        public int QuantityChange { get; set; }

        public int? SrcStockBeforeId { get; set; }
        public virtual Stock SrcStockBefore { get; set; }

        public int? DstStockBeforeId { get; set; }
        public virtual Stock DstStockBefore { get; set; }

        public int? SrcStockAfterId { get; set; }
        public virtual Stock SrcStockAfter { get; set; }

        public int? DstStockAfterId { get; set; }
        public virtual Stock DstStockAfter { get; set; }

        public int StatusValue { get; set; }
        [NotMapped]
        public DetailStatus Status
        {
            get { return (DetailStatus)StatusValue; }
            set { StatusValue = (int)value; }
        }
        public string Note { get; set; }

    }
    public enum DetailStatus
    {
        PENDING,
        ACCEPTED,
        REJECTED
    }
}