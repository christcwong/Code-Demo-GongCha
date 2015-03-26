using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StockWithPendingChangeViewModel
    {
        List<Stock> Stocks;
        Dictionary<Stock, List<TransferOrderDetail>> PendingChanges;
    }
}