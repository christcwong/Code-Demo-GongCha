using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class TransferOrder
    {
        [Key]
        public int Id { get; set; }
        [Timestamp]
        public byte[] LastModified { get; set; }

        public int SourceLocationId { get; set; }
        [DisplayName("Source Location")]
        public virtual Location SourceLocation { get; set; }
        public int DestinationLocationId { get; set; }
        [DisplayName("Destination Location")]
        public virtual Location DestinationLocation { get; set; }
        public int IssuerId { get; set; }
        public virtual Member Issuer { get; set; }
        [DisplayName("Date Of Issue")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime IssueDate { get; set; }

        [DisplayName("Effective Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EffectiveDate { get; set; }
        public string Note { get; set; }
        public int StatusValue { get; set; }
        
        [NotMapped]
        public OrderStatus Status
        {
            get { return (OrderStatus)StatusValue; }
            set { StatusValue = (int)value; }
        }

        public List<TransferOrderDetail> Details { get; set; }  

    }
    public enum OrderStatus
    {
        Draft,
        Active,
        Completed
    }
}