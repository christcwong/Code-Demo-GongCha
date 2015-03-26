using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StockHistory
    {
        [Key]
        public int Id { get; set; }

        public DateTime RecordTime { get; set; }
        public virtual Location Location { get; set; }
        public int LocationId { get; set; }
        public virtual Item Item { get; set; }
        public int ItemId { get; set; }

        [DisplayName("ExpirationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpirationDate { get; set; }

        public int StockBeforeId { get; set; }
        [DisplayName("Stock Before")]
        public virtual Stock StockBefore { get; set; }
        public int StockAfterId { get; set; }
        [DisplayName("Stock After")]
        public virtual Stock StockAfter { get; set; }
        [DisplayName("Quantity Change")]
        public double ChangeQuantity { get; set; }
        public int AuthorizeMemberId { get; set; }
        [DisplayName("Authorize Member")]
        public virtual Member AuthorizeMember { get; set; }
        public int typeValue { get; set; }
        [DisplayName("Type of Change")]
        public StockHistoryChangeType ChangeType
        {
            get { return (StockHistoryChangeType)typeValue; }
            set { this.typeValue = (int)value; }
        }
    }
    public enum StockHistoryChangeType
    {
        PURCHASE,
        EDIT,
        CONSUMPTION,
        TRANSFER,
        EXPIRATION
    }
}