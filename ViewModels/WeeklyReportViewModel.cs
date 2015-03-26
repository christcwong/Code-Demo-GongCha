using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GongChaWebApplication.Models
{
    public class WeeklyReportViewModel
    {
        public int StoreID { get; set; }

        public int StartWeekNumber { get; set; }

        public int EndWeekNumber { get; set; }

        public IEnumerable<Data> DataItems { get; set; }

        public string strFilterSelection { get; set; }


        public class Data
        {
            [DisplayName("Week Number")]
            public int WeekNumber { get; set; }

            [DisplayName("Week Number of Year")]
            public int WeekNumberOfYear { get; set; }

            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; }

            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; }

            [DataType(DataType.Currency), DisplayName("Cash Inflow (Weekly Sales Actual Cash)")]
            public double Inflow { get; set; }

            [DataType(DataType.Currency), DisplayName("Cash Outflow (Wages)")]
            public double WageOutflow { get; set; }

            [DataType(DataType.Currency), DisplayName("Cash Outflow (Bank in)")]
            public double BankOutflow { get; set; }

            [DataType(DataType.Currency), DisplayName("Cash Outflow (Dividend)")]
            public double DividendOutflow { get; set; }

            [DataType(DataType.Currency), DisplayName("Weekly Net Cash")]
            public double CashFlow { get; set; }

            [DataType(DataType.Currency), DisplayName("Accumlative Cash")]
            public double AccumulativeCash { get; set; }
        }
    }
}