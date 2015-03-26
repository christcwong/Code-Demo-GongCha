using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GongChaWebApplication.Models;
using GongChaWebApplication.Controllers;
using System.Data;
using System.Transactions;
using System.Data.Objects.SqlClient;

namespace GongChaWebApplication.Services
{
    public class SalesService : BaseController
    {
        public SalesService(GongChaDbContext db)
        {
            this.db = db;
        }

        public WageType GetWageTypeForDay(int StoreId,DateTime date)
        {
            DateTime dateDate = date.Date;

            var sales = db.SalesItems.Where(
                s =>
                    s.Store.Id == StoreId &&
                    SqlFunctions.DatePart("year", s.Date).Value == dateDate.Year &&
                    SqlFunctions.DatePart("month", s.Date).Value == dateDate.Month &&
                    SqlFunctions.DatePart("day", s.Date).Value == dateDate.Day
                );
            var salesTargets = db.SalesTargets.Where(
                target =>
                    target.StoreId == StoreId &&
                    SqlFunctions.DatePart("year", target.Date).Value == dateDate.Year &&
                    SqlFunctions.DatePart("month", target.Date).Value == dateDate.Month &&
                    SqlFunctions.DatePart("day", target.Date).Value == dateDate.Day
                );
            if (!sales.Any() || !salesTargets.Any())
            {
                // There is nothing the system can do.
                return db.WageTypes.Find(1);
            }
            var sale = sales.Select(s => s.SalesAmount).Max();
            var salesTargetAchieved = salesTargets.Where(t => t.Target <= sale).OrderByDescending(t => t.Target);
            if (!salesTargetAchieved.Any())
            {
                return db.WageTypes.Find(1);
            }
            return salesTargetAchieved.First().WageType;
        }

        
    }
}