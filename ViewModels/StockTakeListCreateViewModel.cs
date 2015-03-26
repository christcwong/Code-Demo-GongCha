using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StockTakeListCreateViewModel
    {
        public Stock dummyStock { get; set; }
        public List<StockTakeListCreateDetailViewModel> Details { get; set; }
    }

    public class StockTakeListCreateDetailViewModel
    {
        public Item Item { get; set; }
        public Stock NewStock { get; set; }
        public List<Stock> ExistingStocks { get; set; }
        public Boolean isModified { get; set; }
    }
}