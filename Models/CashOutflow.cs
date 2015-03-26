using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace GongChaWebApplication.Models
{
    public class CashOutflow
    {
        [Key]
        public int Id { get; set; }

        public virtual Store Store { get; set; }

        public virtual Member Member { get; set; }

        [Timestamp]
        public byte[] LastModified { get; set; }

        public String Type { get; set; }

        [DataType(DataType.Currency), DisplayName("Outflow Amount")]
        public double Outflow { get; set; }

        [DisplayName("Outflow Note")]
        public string OutflowNote { get; set; }

        [Required, DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

    }
}