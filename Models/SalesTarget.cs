using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class SalesTarget
    {
        [Key]
        public int Id { get; set; }

        public int StoreId { get; set; }
        public virtual Store Store { get; set; }

        public int WageTypeId { get; set; }
        [DisplayName("Wage Type")]
        public virtual WageType WageType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DisplayName("Target Amount")]
        public double Target { get; set; }

        public string Note { get; set; }

    }
}