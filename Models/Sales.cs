using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GongChaWebApplication.Models
{
    public class Sales
    {
        [Key]
        public int Id { get; set; }

        public virtual Store Store { get; set; }

        public virtual Member Member { get; set; }

        [Timestamp]
        public byte[] LastModified { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Day { get; set; }

        public string Weather { get; set; }

        [DataType(DataType.MultilineText), DisplayName("General Note / Wasteage Report")]
        public string GeneralNote { get; set; }

        [DataType(DataType.Currency), DisplayName("Sales Amount")]
        public double SalesAmount { get; set; }

        [DataType(DataType.Currency), DisplayName("Cup Sold")]
        public int CupSold { get; set; }

        [DataType(DataType.Currency)]
        public double Refund { get; set; }

        [DataType(DataType.MultilineText), DisplayName("Refund Note")]
        public string RefundNote { get; set; }

        [DataType(DataType.Currency)]
        public double Payout { get; set; }

        [DataType(DataType.MultilineText), DisplayName("Payout Note")]
        public string PayoutNote { get; set; }

        [DataType(DataType.Currency), DisplayName("Expect Cash")]
        public double ExpectCash { get; set; }

        [DataType(DataType.Currency), DisplayName("Actual Cash")]
        public double ActualCash { get; set; }

        [DataType(DataType.Currency), DisplayName("Discrepancy")]
        public double Difference { get; set; }

        public string Status { get; set; }               

        // Alias -- Not working with code first migration, removed now.
        // public double CashInHand { get { return this.ActualCash; } set { this.ActualCash = value; } }
    }
}