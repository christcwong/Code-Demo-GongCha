﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class CashOutflowViewModel
    {
        public IEnumerable<GongChaWebApplication.Models.CashOutflow> CashOutflowItems { get; set; }
    }
}