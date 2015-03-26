using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GongChaWebApplication.Models;
using GongChaWebApplication.Controllers;
using System.Data;
using System.Transactions;
using GongChaWebApplication.Helpers;


namespace GongChaWebApplication.Services
{
    public class StockService : BaseController
    {
        //private GongChaDbContext db;

        //public StockService()
        //{
        //    this.db = new GongChaDbContext();
        //}

        private TransactionScope scope;

        public StockService(GongChaDbContext db)
        {
            this.db = db;
            this.scope = null;
        }

        #region Transaction Manager
        public void BeginTransaction()
        {
            if (this.scope != null)
            {
                throw new InvalidOperationException("There is already a transaction.");
            }
            this.scope = new TransactionScope();
            return;
        }
        public int CommitChanges()
        {
            if (this.scope == null)
            {
                return db.SaveChanges();
            }
            scope.Complete();
            this.scope = null;
            return db.SaveChanges();
        }
        #endregion

        #region Stock Get / Set

        public Stock GetLastestStocksWithExpirationDate(int LocationId, int ItemId, DateTime ExpirationDate)
        {
            var stockHistories = db.StockHistories.Where(
                    h =>
                        h.LocationId == LocationId &&
                        h.ItemId == ItemId &&
                        h.ExpirationDate == ExpirationDate
                ).OrderByDescending(h => h.RecordTime);
            if (stockHistories.Any())
            {
                return db.Stocks.Where(s => s.Id == stockHistories.FirstOrDefault().StockAfterId).First();
            }
            return null;
        }

        public IQueryable<Stock> GetActiveStocks(int LocationId, int ItemId)
        {

            var stocks = db.Stocks
                .Where(s =>
                    s.LocationId == LocationId &&
                    s.ItemId == ItemId &&
                    s.stateValue == (int)StockState.CONFIRMED)
                .OrderBy(s => s.ExpirationDate)
                .ThenByDescending(s => s.Date)
                .ThenByDescending(s => s.LastModified);
            return stocks;
        }
        #endregion

        #region Stock Create/Update In Single Transaction (Should not be mixed with other db.SaveChanges())
        public void StockPurchase(Stock stock)
        {
            var existingStocks = GetPreviousStocks(stock);
            if (existingStocks.Any())
            {
                var existingAmount = existingStocks.Sum(s => s.Quantity);
                stock.Quantity += existingAmount;
            }
            StockUpdate(stock, StockHistoryChangeType.PURCHASE, "Stock Purchased. ");
        }

        public void StockEdit(Stock stock)
        {
            Stock newStock = CloneStock(stock);
            StockUpdate(newStock, StockHistoryChangeType.EDIT, "Stock Edited. ");
        }

        public void StockConsume(Stock stock)
        {
            Stock newStock = CloneStock(stock);
            StockUpdate(newStock, StockHistoryChangeType.CONSUMPTION, "Stock Consumed. ");
        }

        public void StockExpire(Stock stock)
        {
            Stock newStock = CloneStock(stock);
            newStock.Quantity = 0;
            newStock.stateValue = (int)StockState.EXPIRED;
            StockUpdate(newStock, StockHistoryChangeType.EXPIRATION, "Stock Expired after Expiration Date. ");
        }

        public void StockTransfer(Stock stock)
        {
            stock.stateValue = (int)StockState.EXPIRED;
            StockUpdate(stock, StockHistoryChangeType.CONSUMPTION, "Stock Expired by Transfering. ");
        }

        public void StockTake(Stock stock)
        {
            Stock newStock = CloneStock(stock);
            StockUpdate(newStock, StockHistoryChangeType.CONSUMPTION, "Stock updated by StockTake. ");
        }

        /// <summary>
        /// Update Stock for stock specificed by newStock.
        /// New StockHistory will be inserted into database.
        /// Latest Previous Stock with the same ItemId,LocationId and Expiration Date Specified by newStock will be marked in the History
        /// </summary>
        /// <param name="newStock">New version of Stock that will be put into database</param>
        private void StockUpdate(Stock newStock, StockHistoryChangeType updateType, string noteToAdd)
        {
            if (this.scope != null)
            {
                using (this.scope)
                {
                    InsertStock(newStock, noteToAdd);

                    ExpirePreviousStocks(newStock, noteToAdd);
                    // no need to update if there is no previous Stock.

                    var latestPreviousStock = GetLastestStocksWithExpirationDate(
                            newStock.LocationId,
                            newStock.ItemId,
                            newStock.ExpirationDate
                        );

                    InsertStockChangeHistory(newStock, latestPreviousStock, updateType);

                    // Update DB
                    db.SaveChanges();
                }
            }
            else
            {
                InsertStock(newStock, noteToAdd);

                ExpirePreviousStocks(newStock, noteToAdd);
                // no need to update if there is no previous Stock.

                var latestPreviousStock = GetLastestStocksWithExpirationDate(
                        newStock.LocationId,
                        newStock.ItemId,
                        newStock.ExpirationDate
                    );

                InsertStockChangeHistory(newStock, latestPreviousStock, updateType);

                // Update DB
                db.SaveChanges();
            }
        }







        #endregion

        #region Stock Expire

        public void UpdateExpiredStocks(int LocationId = 0)
        {
            Location loc = db.Locations.Find(LocationId);
            IQueryable<Stock> expiredStocks;
            if (loc == null)
            {
                expiredStocks = db.Stocks.Where(s =>
                    s.ExpirationDate <= GongChaDateTimeWrapper.Now &&
                    s.stateValue == (int)StockState.CONFIRMED
                    );
            }
            else
            {
                expiredStocks = db.Stocks.Where(s =>
                    s.LocationId == LocationId &&
                    s.ExpirationDate <= GongChaDateTimeWrapper.Now &&
                    s.stateValue == (int)StockState.CONFIRMED
                    );
            }
            foreach (var stock in expiredStocks.ToList())//Force it to materialize the search.
            {
                StockExpire(stock);
            }
            db.SaveChanges();
        }
        #endregion

        #region Stock Consumption

        #endregion

        #region helpers
        public Stock CloneStock(Stock pStock)
        {
            if (pStock == null)
            {
                return new Stock()
                {
                    //Checker = CurrentMember,
                    //MemberId = CurrentMember.Id,
                    Date = GongChaDateTimeWrapper.Now,
                    ExpirationDate = GongChaDateTimeWrapper.Now,
                    Quantity = 0,
                    stateValue = (int)StockState.PENDING
                };
            }
            Stock Clone = new Stock()
            {
                Checker = pStock.Checker,
                MemberId = pStock.MemberId,
                Date = pStock.Date,
                ExpirationDate = pStock.ExpirationDate, //Notice it is using the same date from SrcStock
                Item = pStock.Item,
                ItemId = pStock.ItemId,
                Location = pStock.Location,
                LocationId = pStock.LocationId,
                Quantity = pStock.Quantity,
                stateValue = pStock.stateValue,
                State = pStock.State,
                //LastModified = pStock.LastModified
            };
            return Clone;
        }
        public void ExpirePreviousStocks(Stock newStock, string noteToAdd)
        {
            // Find the all Previous Stock of same expiration date.
            var previousStocks = GetPreviousStocks(newStock);

            // Update latest Previous Stock
            if (previousStocks.Any())
            {
                foreach (var stock in previousStocks.ToList())
                {
                    stock.stateValue = (int)StockState.EXPIRED;
                    stock.Note += "Expired at " + GongChaDateTimeWrapper.Now.ToString() + ". " + noteToAdd;
                    //db.Entry(stock).State = EntityState.Modified;
                }
            }
        }

        private IQueryable<Stock> GetPreviousStocks(Stock newStock)
        {
            var previousStocks = db.Stocks
                .Where(s =>
                    s.ItemId == newStock.ItemId &&
                    s.LocationId == newStock.LocationId &&
                    s.ExpirationDate == newStock.ExpirationDate &&
                    s.stateValue == (int)StockState.CONFIRMED
                );
            return previousStocks;
        }
        public void InsertStock(Stock newStock, string noteToAdd)
        {
            newStock.Note += noteToAdd;

            // Insert New Stock
            db.Stocks.Add(newStock);
        }

        public void InsertStockChangeHistory(Stock newStock, Stock latestPreviousStock, StockHistoryChangeType updateType)
        {
            // Insert StockHistory
            StockHistory shist = new StockHistory()
            {
                RecordTime = GongChaDateTimeWrapper.Now,
                LocationId = newStock.LocationId,
                ItemId = newStock.ItemId,
                ExpirationDate = newStock.ExpirationDate,
                ChangeType = updateType,
                typeValue = (int)updateType,
                //AuthorizeMember = newStock.Checker,
                AuthorizeMemberId = newStock.MemberId,
                ChangeQuantity = newStock.Quantity - (latestPreviousStock == null ? 0 : latestPreviousStock.Quantity),
                // Need to check null
                //StockBefore = latestPreviousStock,
                StockBeforeId = (latestPreviousStock == null || latestPreviousStock.Id == 0 ? newStock.Id : latestPreviousStock.Id),
                //StockAfter = newStock,
                StockAfterId = newStock.Id
            };
            db.StockHistories.Add(shist);
        }
        #endregion
    }
}