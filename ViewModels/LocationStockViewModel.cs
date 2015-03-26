using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class LocationStockViewModel
    {
        public Location Location { get; set; }
        public Item Item { get; set; }
        public List<StockViewModel> Stocks { get; set; }
    }
}