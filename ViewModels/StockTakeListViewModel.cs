using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StockTakeListViewModel
    {
        public List<Stock> Stocks { get; set; }
        public List<TransferOrderDetail> PendingTransferOrderDetails { get; set; }
    }
}