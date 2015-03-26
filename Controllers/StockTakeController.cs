using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using GongChaWebApplication.Models;
using GongChaWebApplication.Services;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Controllers
{
    [Authorize]
    public class StockTakeController : BaseController
    {

        //
        // GET: /StockTake/
        StockService stockService;

        public StockTakeController()
            : base()
        {
            this.stockService = new StockService(this.db);
        }

        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(db.Locations.ToList());
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return View(db.Warehouses.Select(wh => wh.Location).Distinct().ToList());
            }
            return RedirectToAction("List", new { id = CurrentMember.Store.LocationId });
            //return View(db.Stores.Where(s => s.Id == CurrentMember.Store.Id).Select(s => s.Location).ToList());
        }

        public ActionResult List(int id)
        {
            CurrentLocation = db.Locations.Where(loc => loc.Id == id).FirstOrDefault();
            if (CurrentMember == null)
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (CurrentLocation != null)
            {
                ViewBag.Location = CurrentLocation;
                var stocks = db.Stocks
                                .Where(s => s.LocationId == id)
                                .OrderByDescending(s => s.Date)
                                .ThenByDescending(s => s.LastModified)
                                .ThenBy(s => s.stateValue)
                                .Include(s => s.Item)
                                .Include(s => s.Location)
                                .Include(s => s.Checker);
                var pendingTransferOrderDetails = db.TransferOrderDetails
                                                    .Where(detail => detail.ParentOrder.DestinationLocationId == CurrentLocation.Id && detail.StatusValue == (int)DetailStatus.PENDING)
                                                    .Include(detail => detail.SrcStockBefore)
                                                    .Include(detail => detail.Item);
                StockTakeListViewModel vm = new StockTakeListViewModel
                {
                    Stocks = stocks.ToList(),
                    PendingTransferOrderDetails = pendingTransferOrderDetails.ToList()
                };
                return View(vm);
            }
            return HttpNotFound();
        }

        //
        // GET: /StockTake/Details/5

        public ActionResult Details(int id = 0)
        {
            Stock stock = db.Stocks.Find(id);
            CurrentLocation = db.Locations.Find(stock.LocationId);
            if (CurrentMember == null)
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        public ActionResult ListCreate()
        {
            //#if DEBUG
            //            CurrentMember = db.Members.Find(4);
            //            CurrentLocation = db.Locations.Find(1);
            //#endif
            if (CurrentMember == null)
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (CurrentLocation == null)
            {
                return RedirectToAction("Index");
            }
            List<StockTakeListCreateDetailViewModel> detailList = new List<StockTakeListCreateDetailViewModel>();
            var allItems = db.Items.ToList();

            foreach (var item in allItems)
            {
                var stocks = db.Stocks
                                    .Where(
                                        s =>
                                            s.ItemId == item.Id &&
                                            s.LocationId == CurrentLocation.Id &&
                                            s.stateValue == (int)StockState.CONFIRMED &&
                                            s.Quantity != 0
                                    )
                                    .OrderByDescending(s => s.Date)
                                    .ThenByDescending(s => s.LastModified);
                if (stocks.Any())
                {
                    Stock stock = new Stock();
                    stock.Item = item;
                    stock.ItemId = item.Id;
                    stock.Checker = CurrentMember;
                    stock.MemberId = CurrentMember.Id;
                    stock.Date = GongChaDateTimeWrapper.Now;
                    stock.ExpirationDate = GongChaDateTimeWrapper.Now;
                    stock.Location = CurrentLocation;
                    stock.LocationId = CurrentLocation.Id;
                    stock.Quantity = 0;
                    detailList.Add(new StockTakeListCreateDetailViewModel()
                    {
                        Item = item,
                        NewStock = stock,
                        ExistingStocks = stocks.ToList(),
                        isModified = false
                    });
                }
            }
            StockTakeListCreateViewModel vm = new StockTakeListCreateViewModel()
            {
                Details = detailList,
                dummyStock = new Stock() { Date = GongChaDateTimeWrapper.Now }
            };
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Serial");
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name");
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Email");
            return View(vm);
        }
        [HttpPost]
        public ActionResult ListCreate(StockTakeListCreateViewModel vm)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (ModelState.IsValid)
            {
                // Perform Validation
                if (vm.Details != null)
                {
                    foreach (var detail in vm.Details)
                    {
                        var allstocks = (detail.ExistingStocks != null ? detail.ExistingStocks : new List<Stock>()).Union((detail.NewStock != null ? new List<Stock> { detail.NewStock } : new List<Stock>()));
                        var expDateGrp = allstocks.GroupBy(s => s.ExpirationDate);
                        if (expDateGrp.Any(grp => grp.Count() > 1))
                        {
                            PrepareViewBag();
                            var item = db.Items.Find(allstocks.First(s => s.ItemId != 0).ItemId);
                            ViewBag.StatusMessage += "There are duplicated expiration date for item " + new ItemViewModel(item).Label + ". Please edit the existing item of following dates instead :";
                            var duplicatedDates = expDateGrp.Where(grp => grp.Count() > 1).Select(grp => grp.Key.ToShortDateString()).ToList();
                            foreach (var duplicatedDate in duplicatedDates)
                            {
                                ViewBag.StatusMessage += " " + duplicatedDate.ToString() + " ";
                            }
                            // fill the item details back into the model...
                            foreach (var missingItemDetail in vm.Details)
                            {
                                var tmpItem = db.Items.Find(missingItemDetail.NewStock.ItemId);
                                missingItemDetail.Item = tmpItem;
                                missingItemDetail.NewStock.Item = tmpItem;
                            }
                            vm.dummyStock = new Stock() { Date = GongChaDateTimeWrapper.Now };
                            return View(vm);
                        }
                    }

                    // Submit changes
                    using (TransactionScope scope = new TransactionScope())
                    {
                        foreach (var detail in vm.Details)
                        {
                            if (detail.NewStock != null && detail.NewStock.Quantity != 0)
                            {
                                detail.NewStock.Date = vm.dummyStock.Date;
                                detail.NewStock.LocationId = CurrentLocation.Id;
                                detail.NewStock.MemberId = CurrentMember.Id;
                                stockService.StockTake(detail.NewStock);
                            }
                            if (detail.ExistingStocks != null)
                            {
                                foreach (var existingStock in detail.ExistingStocks)
                                {
                                    var dbStock = db.Stocks.Find(existingStock.Id);
                                    if (dbStock != null)
                                    {
                                        // no matter what, add stock take record.
                                        var newStock = stockService.CloneStock(dbStock);
                                        newStock.MemberId = CurrentMember.Id;
                                        newStock.Quantity = existingStock.Quantity;
                                        stockService.StockTake(newStock);
                                    }
                                }
                            }

                        }
                        scope.Complete();
                    };
                };
            }
            PrepareViewBag();
            return RedirectToAction("List", new { id = CurrentLocation.Id });
        }

        //
        // GET: /StockTake/Create

        public ActionResult Create()
        {
            if (CurrentLocation == null)
            {
                return RedirectToAction("Index");
            }
            if (!Accessible())
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            PrepareViewBag();


            Stock stock = new Stock
            {
                Date = GongChaDateTimeWrapper.Now,
                ExpirationDate = GongChaDateTimeWrapper.Now,
                //Checker = CurrentMember,
                MemberId = CurrentMember.Id
            };
            if (CurrentLocation != null)
            {
                stock.LocationId = CurrentLocation.Id;
                stock.Location = CurrentLocation;
            }
            return View(stock);
        }

        //
        // POST: /StockTake/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Stock stock)
        {
            if (!Accessible())
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (ModelState.IsValid)
            {
                stockService.StockPurchase(stock);
                stockService.CommitChanges();
                //db.Stocks.Add(stock);
                //db.SaveChanges();
                return RedirectToAction("List", new { Id = CurrentLocation.Id });
            }
            stock.Location = db.Locations.Find(stock.LocationId);
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Serial", stock.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", stock.LocationId);
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Email", stock.MemberId);
            return View(stock);
        }

        //
        // GET: /StockTake/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Stock stock = db.Stocks.Find(id);
            if (stock != null)
            {
                CurrentLocation = db.Locations.Find(stock.LocationId);
            }
            if (!Accessible())
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (stock == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return View(stock);
        }

        //
        // POST: /StockTake/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Stock stock)
        {
            if (!Accessible())
            {
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            if (ModelState.IsValid)
            {
                //Stock editedStock = CloneStock(stock);
                //Stock oldStock = db.Stocks.Find(stock.Id);
                //oldStock.stateValue = (int)StockState.EXPIRED;
                //oldStock.Note += " Replaced by amount Edited.";
                //editedStock.Note += " Amount Edited.";
                //if (oldStock.ExpirationDate != editedStock.ExpirationDate)
                //{
                //    oldStock.Note += " Expiration Date Edited.";
                //    editedStock.Note += " Created by expiration date edit.";
                //}
                //db.Stocks.Add(editedStock);
                //db.Entry(oldStock).State = EntityState.Modified;
                ////db.Entry(stock).State = EntityState.Modified;
                //db.SaveChanges();
                stockService.StockEdit(stock);
                stockService.CommitChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Serial", stock.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", stock.LocationId);
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", stock.MemberId);
            return View(stock);
        }

        public ActionResult History(int ItemId = 0, int LocationId = 0)
        {
            CurrentLocation = db.Locations.Find(LocationId);
            if (!Accessible())
            {
                CurrentLocation = null;
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            var stocks = db.Stocks.Where(s => s.ItemId == ItemId && s.LocationId == LocationId).OrderByDescending(s => s.Date).ThenByDescending(s => s.LastModified);
            PrepareViewBag();
            if (stocks.Any())
            {
                return View(stocks.Include(s => s.Item).Include(s => s.Location).Include(s => s.Checker).ToList());
            }
            return View(new List<Stock> { 
                new Stock{
                    Item=db.Items.Find(ItemId),
                    LocationId=LocationId,
                    Location=db.Locations.Find(LocationId),
                    State=StockState.CONFIRMED,
                    Note = "There is no stock for this item yet."
                }
            });
        }


        ////
        //// GET: /StockTake/Delete/5

        //public ActionResult Delete(int id = 0)
        //{
        //    Stock stock = db.Stocks.Find(id);
        //    if (stock == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(stock);
        //}

        ////
        //// POST: /StockTake/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Stock stock = db.Stocks.Find(id);
        //    db.Stocks.Remove(stock);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}


        #region private helpers

        private void PrepareViewBag()
        {
            var ItemViewList = new List<ItemViewModel>();
            foreach (var item in db.Items)
            {
                ItemViewList.Add(new ItemViewModel(item));
            }
            ViewBag.ItemId = new SelectList(ItemViewList, "Id", "Label");
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name");
            ViewBag.PackageUnitId = new SelectList(db.Units.Where(u => u.unitType == UnitType.Package), "Id", "Symbol");
        }

        public Stock CloneStock(Stock pStock)
        {
            if (pStock == null)
            {
                return new Stock()
                {
                    //Checker = CurrentMember,
                    MemberId = CurrentMember.Id,
                    Date = GongChaDateTimeWrapper.Now,
                    ExpirationDate = GongChaDateTimeWrapper.Now,
                    Quantity = 0,
                    stateValue = (int)StockState.PENDING
                };
            }
            Stock Clone = new Stock()
            {
                //Checker = CurrentMember,
                MemberId = CurrentMember.Id,
                Date = pStock.Date,
                ExpirationDate = pStock.ExpirationDate, //Notice it is using the same date from SrcStock
                //Item = pStock.Item,
                ItemId = pStock.ItemId,
                //Location = pStock.Location,
                LocationId = pStock.LocationId,
                Quantity = pStock.Quantity,
                stateValue = pStock.stateValue,
                State = pStock.State
            };
            return Clone;
        }
        #endregion
    }
}