using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace GongChaWebApplication.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }
        [Timestamp]
        public byte[] LastModified { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        public double Quantity { get; set; }

        public int ItemId { get; set; }
        public virtual Item Item { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        [DisplayName("Expiration Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpirationDate { get; set; }

        public String Note { get; set; }

        public int MemberId { get; set; }
        [ScriptIgnore]
        public virtual Member Checker { get; set; }

        public int stateValue { get; set; }
        [NotMapped]
        [DisplayName("State")]
        public virtual StockState State { get { return (StockState)stateValue; } set { this.stateValue = (int)value; } }
    }
    public enum StockState
    {
        CONFIRMED,
        PENDING,
        REJECTED,
        EXPIRED
    }
}