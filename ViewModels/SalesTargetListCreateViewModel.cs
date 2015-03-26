using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class SalesTargetListCreateViewModel
    {
        public int StoreId { get; set; }
        public Store Store { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        public List<WageType> WageTypes { get; set; }
        public List<SalesTarget> SalesTargetsForWageType { get; set; }
    }
}