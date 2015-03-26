using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StockCheckViewModel
    {
        public int alertDays { get; set; }
        public Location location { get; set; }
        public List<Unit> allPackageUnits { get; set; }
        public List<Stock> stocks { get; set; }
        public Stock dummyStock { get; set; }
        public Dictionary<Stock, List<TransferOrderDetail>> PendingChanges{get;set;}
    }
}