using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class SalesListViewModel
    {
        public IEnumerable<GongChaWebApplication.Models.Sales> SaleItems { get; set; }
    }
}