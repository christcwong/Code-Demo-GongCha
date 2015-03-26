using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GongChaWebApplication.Models
{
    public class DailyReportViewModel 
    {
        public string TypeName = "Daily Report";

        public int StoreID { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public IEnumerable<Data> DataItems { get; set; }

        public string strFilterSelection { get; set; }

        public class Data
        {
            [DataType(DataType.Date)]
            public DateTime Date { get; set; }

            public string Day { get; set; }

            public string Weather { get; set; }

            [DataType(DataType.Currency), DisplayName("POS Sales Amount")]
            public double SalesAmount { get; set; }

            [DataType(DataType.Currency)]
            public double ActualCash { get; set; }

            [DataType(DataType.Currency)]
            public double Payout { get; set; }

            [DataType(DataType.MultilineText), DisplayName("Payout Note")]
            public string PayoutNote { get; set; }

            [DataType(DataType.Currency), DisplayName("Discrepancy")]
            public double Difference { get; set; }

            public string OutflowType { get; set; }

            [DataType(DataType.Currency), DisplayName("Cash Outflow")]
            public double Outflow { get; set; }

            public Member Member { get; set; }

        }
    }


}