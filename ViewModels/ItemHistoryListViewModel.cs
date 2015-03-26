using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class ItemHistoryListViewModel
    {
        public Location Location { get; set; }
        public List<Item> Items { get; set; }
        public Dictionary<Item, List<StockHistory>> ItemStockHistories { get; set; }
        public StockHistory dummyStockHistory{get;set;}
    }
}