using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class TransferOrderCreateViewModel
    {
        public TransferOrder NewOrder { get; set; }
        public TransferOrderDetailCreateViewModel NewDetail { get; set; }
        public List<TransferOrderDetailCreateViewModel> Details { get; set; }
    }
}