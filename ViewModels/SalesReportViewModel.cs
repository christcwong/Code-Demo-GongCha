using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GongChaWebApplication.Models
{
    public class SalesReportViewModel
    {
        public int StoreID { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string FilterSelection { get; set; }

        public IEnumerable<DailyReportViewModel.Data> dailyReportData;

        public IEnumerable<WeeklyReportViewModel.Data> weeklyReportData;

        [DataType(DataType.Currency), DisplayName("Total sales after payout")]
        public double TotalSalesAfterPayout { get; set; }

        [DataType(DataType.Currency), DisplayName("Balance Cash")]
        public double TotalCashReceived { get; set; }

        [DataType(DataType.Currency), DisplayName("Total wages")]
        public double TotalWage { get; set; }

        [DataType(DataType.Currency), DisplayName("Total deposit to bank")]
        public double TotalDeposit { get; set; }

        [DataType(DataType.Currency), DisplayName("Total dividend")]
        public double TotalDividend { get; set; }

        [DataType(DataType.Currency), DisplayName("Cash in hand")]
        public double TotalAccumulativeCash { get; set; }
    }
}