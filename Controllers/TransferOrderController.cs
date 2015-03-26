using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using GongChaWebApplication.Services;
using System.Transactions;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Controllers
{
#if RELEASE
    [Authorize]
#endif
    public class TransferOrderController : BaseController
    {
        StockService stockService;
        public TransferOrderController()
            : base()
        {
            this.stockService = new StockService(this.db);
        }

        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            var transferorder = db.TransferOrders.Select(o => o);
            if (CurrentMember.AccessLevel != "director")
            {
                var whLocIds = db.Warehouses.Select(wh => wh.LocationId);
                var accessibleLocIds = db.Stores.Where(s => s.Id == CurrentMember.Store.Id).Select(s => s.LocationId).Union(whLocIds).Distinct();
                transferorder = transferorder.Where(order => accessibleLocIds.Contains(order.DestinationLocationId) || accessibleLocIds.Contains(order.SourceLocationId));
            }
            transferorder = transferorder.OrderByDescending(t => t.IssueDate).ThenByDescending(t => t.Id).Include(t => t.SourceLocation).Include(t => t.DestinationLocation).Include(t => t.Issuer);
            return View(transferorder.ToList());
        }

        //
        // GET: /TransferOrder/Details/5

        public ActionResult Details(int id = 0)
        {
            TransferOrder transferorder = db.TransferOrders.Find(id);
            if (transferorder == null)
            {
                return HttpNotFound();
            }
            transferorder.Details = db.TransferOrderDetails.Where(detail => detail.ParentOrderID == id).ToList();
            return View(transferorder);
        }

        //
        // GET: /TransferOrder/Create

        public ActionResult Create()
        {
            //#if DEBUG
            //            CurrentMember = (from m in db.Members where m.Id == 1 select m).First();
            //#endif
            if (CurrentMember == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Login", "Member", new { ReturnUrl = Request.Path });
            }
            TransferOrder transferorder = new TransferOrder()
                {
                    IssuerId = CurrentMember.Id,
                    SourceLocationId = (CurrentMember.Store == null ? 0 : CurrentMember.Store.LocationId),
                    IssueDate = GongChaDateTimeWrapper.Now.Date,
                    EffectiveDate = GongChaDateTimeWrapper.Now.Date,
                    Status = OrderStatus.Draft,
                    Details = new List<TransferOrderDetail>()
                };
            TransferOrderCreateViewModel vm = new TransferOrderCreateViewModel()
            {
                NewOrder = transferorder,
                Details = new List<TransferOrderDetailCreateViewModel>(),
                NewDetail = new TransferOrderDetailCreateViewModel() { detail = new TransferOrderDetail() { ParentOrderID = transferorder.Id }, markDelete = false }
            };
            vm.Details.Add(vm.NewDetail);
            var whLocs = db.Warehouses.Include(wh => wh.Location).Select(join => join.Location);
            var accessibleLocs = ((CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER) ? db.Locations : db.Stores.Where(s => s.Id == CurrentMember.Store.Id).Include(s => s.Location).Select(join => join.Location).Union(whLocs).Distinct());
            ViewBag.SourceLocationId = new SelectList(accessibleLocs, "Id", "Name");
            ViewBag.DestinationLocationId = new SelectList(db.Locations, "Id", "Name");
            ViewBag.IssuerId = new SelectList(db.Members, "Id", "Email");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransferOrderCreateViewModel vm)
        {
            var form = Request.Form;
            if (ModelState.IsValid)
            {
                vm.Details.RemoveAll(detail => detail.markDelete == true || detail.detail.ItemId == 0);
                db.TransferOrders.Add(vm.NewOrder);
                db.SaveChanges();
                foreach (var detail in vm.Details)
                {
                    var currentDetail = detail.detail;
                    currentDetail.ParentOrderID = vm.NewOrder.Id;
                    currentDetail.StatusValue = (int)DetailStatus.PENDING;
                    currentDetail.Status = DetailStatus.PENDING;
                    currentDetail.SrcStockBefore = db.Stocks.Find(currentDetail.SrcStockBeforeId);
                    // Check if Dst Location has stock with that expiration date. if no, create an stock with Quantity 0.
                    var dstStock = stockService.GetActiveStocks(vm.NewOrder.DestinationLocationId, currentDetail.ItemId);
                    if (!dstStock.Where(s => s.ExpirationDate == currentDetail.SrcStockBefore.ExpirationDate).Any())
                    {
                        Stock newStockForDst = stockService.CloneStock(currentDetail.SrcStockBefore);
                        newStockForDst.LocationId = vm.NewOrder.DestinationLocationId;
                        newStockForDst.Location = db.Locations.Find(vm.NewOrder.DestinationLocationId);
                        newStockForDst.Quantity = 0;
                        db.Stocks.Add(newStockForDst);
                    }
                    db.TransferOrderDetails.Add(currentDetail);

                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SourceLocationId = new SelectList(db.Locations, "Id", "Name", vm.NewOrder.SourceLocationId);
            ViewBag.DestinationLocationId = new SelectList(db.Locations, "Id", "Name", vm.NewOrder.DestinationLocationId);
            ViewBag.IssuerId = new SelectList(db.Members, "Id", "Email", vm.NewOrder.IssuerId);
            return View(vm);
        }

        //
        // GET: /TransferOrder/Edit/5

        public ActionResult Edit(int id = 0)
        {
            TransferOrder transferorder = db.TransferOrders.Find(id);
            if (transferorder == null)
            {
                return HttpNotFound();
            }
            TransferOrderCreateViewModel vm = new TransferOrderCreateViewModel()
            {
                NewOrder = transferorder,
                Details = db.TransferOrderDetails.Where(d => d.ParentOrderID == id)
                            .Include(d => d.SrcStockBefore)
                            .Include(d => d.DstStockBefore)
                            .Include(d => d.SrcStockAfter)
                            .Include(d => d.DstStockAfter)
                            .Select(s => new TransferOrderDetailCreateViewModel { detail = s, markDelete = false }).ToList(),
                NewDetail = new TransferOrderDetailCreateViewModel()
            };
            var whLocs = db.Warehouses.Include(wh => wh.Location).Select(join => join.Location);
            var accessibleLocs = db.Stores.Where(s => s.Id == CurrentMember.Store.Id).Include(s => s.Location).Select(join => join.Location).Union(whLocs).Distinct();
            ViewBag.SourceLocationId = new SelectList(accessibleLocs, "Id", "Name");
            ViewBag.DestinationLocationId = new SelectList(db.Locations, "Id", "Name", transferorder.DestinationLocationId);
            ViewBag.IssuerId = new SelectList(db.Members, "Id", "Email", transferorder.IssuerId);
            return View(vm);
        }

        //
        // POST: /TransferOrder/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TransferOrderCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vm.NewOrder).State = EntityState.Modified;
                var removableItems = vm.Details.Where(detail => detail.markDelete == true || detail.detail.ItemId == 0).Select(s => s.detail);
                foreach (var detail in vm.Details)
                {
                    detail.detail.ParentOrderID = vm.NewOrder.Id;
                    db.Entry(detail.detail).State = EntityState.Modified;
                    if (!db.TransferOrderDetails.Where(d => d.Id == detail.detail.Id).Any())
                    {
                        db.TransferOrderDetails.Add(detail.detail);
                    }
                }
                foreach (var item in removableItems)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.TransferOrderDetails.Remove(item);
                }
                vm.Details.RemoveAll(detail => detail.markDelete == true || detail.detail.ItemId == 0);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SourceLocationId = new SelectList(db.Locations, "Id", "Name", vm.NewOrder.SourceLocationId);
            ViewBag.DestinationLocationId = new SelectList(db.Locations, "Id", "Name", vm.NewOrder.DestinationLocationId);
            ViewBag.IssuerId = new SelectList(db.Members, "Id", "Email", vm.NewOrder.IssuerId);
            return View(vm);
        }

        //
        // GET: /TransferOrder/Delete/5

        public ActionResult Delete(int id = 0)
        {
            TransferOrder transferorder = db.TransferOrders.Find(id);
            if (transferorder == null)
            {
                return HttpNotFound();
            }
            return View(transferorder);
        }

        //
        // POST: /TransferOrder/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TransferOrder transferorder = db.TransferOrders.Find(id);
            db.TransferOrders.Remove(transferorder);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX call to return a TransferOrderDetail Object
        /// </summary>
        /// <param name="Id">The Row Index that will be put into the ajax reply</param>
        /// <returns></returns>
        public ActionResult NewTransferOrderDetail(int Id)
        {
            var detail = new TransferOrderDetailCreateViewModel { detail = new TransferOrderDetail() };
            //ViewBag.RowId = Id;
            return PartialView("EditorTemplates/TransferOrderDetailCreateViewModel", detail);
        }


        #region Accept/Reject TransferOrder
        public ActionResult Accept(int TransferOrderDetailId)
        {
            TransferOrderDetail od = db.TransferOrderDetails.Find(TransferOrderDetailId);
            if (od == null)
            {
                return HttpNotFound();
            }
            using (TransactionScope scope = new TransactionScope())
            {
                Stock srcSelectedStockForTransfer = db.Stocks.Find(od.SrcStockBeforeId);

                // Get the stock that will be modified at Source Location
                Stock srcLatestStockWithSameExpirationDate
                    = stockService.GetLastestStocksWithExpirationDate(
                        srcSelectedStockForTransfer.LocationId,
                        srcSelectedStockForTransfer.ItemId,
                        srcSelectedStockForTransfer.ExpirationDate
                    );

                var newSrcStock = stockService.CloneStock(srcLatestStockWithSameExpirationDate);
                //newSrcStock = new Stock() { };

                Stock dstLatestStockWithSameExpirationDate = stockService.GetLastestStocksWithExpirationDate(
                        od.ParentOrder.DestinationLocationId,
                        srcSelectedStockForTransfer.ItemId,
                        srcSelectedStockForTransfer.ExpirationDate
                    );

                if (dstLatestStockWithSameExpirationDate == null)
                {
                    // if there is no such stock, create one with quantity 0
                    dstLatestStockWithSameExpirationDate = stockService.CloneStock(srcLatestStockWithSameExpirationDate);
                    dstLatestStockWithSameExpirationDate.Quantity = 0;
                    dstLatestStockWithSameExpirationDate.LocationId = od.ParentOrder.DestinationLocationId;
                    dstLatestStockWithSameExpirationDate.Location = null;
                    //dstLatestStockWithSameExpirationDate.Location = db.Locations.Find(dstLatestStockWithSameExpirationDate.LocationId);
                    dstLatestStockWithSameExpirationDate.MemberId = CurrentMember.Id;
                    //dstLatestStockWithSameExpirationDate.Checker = CurrentMember;
                    stockService.InsertStock(dstLatestStockWithSameExpirationDate, "Auto-Created Stock for empty Stock");
                    stockService.CommitChanges();
                }

                Stock newDstStock = stockService.CloneStock(dstLatestStockWithSameExpirationDate);
                //newDstStock = new Stock() { };
                //Stock newSrcStock = CloneStock(srcLatestStockWithSameExpirationDate);

                var acceptTime = GongChaDateTimeWrapper.Now;
                newSrcStock.State = StockState.CONFIRMED;
                newSrcStock.Note = "Auto-Created Stock Record from Transfer Order. Accepted By " + CurrentMember.Name + " at " + acceptTime;
                newSrcStock.Quantity -= od.QuantityChange;
                newSrcStock.Date = GongChaDateTimeWrapper.Now;
                newSrcStock.Checker = null;
                newSrcStock.MemberId = CurrentMember.Id;

                newDstStock.State = StockState.CONFIRMED;
                newDstStock.Note = "Auto-Created Stock Record from Transfer Order. Accepted By " + CurrentMember.Name + " at " + acceptTime;
                newDstStock.Quantity += od.QuantityChange;
                newDstStock.Date = GongChaDateTimeWrapper.Now;
                newDstStock.Checker = null;
                newDstStock.MemberId = CurrentMember.Id;


                // Since I need to hold the db.SaveChanges at last, I have no choice but to write the codes out again...
                //stockService.StockExpire(srcLatestStockWithSameExpirationDate);
                stockService.ExpirePreviousStocks(srcLatestStockWithSameExpirationDate, "By Transfer Order " + od.Id);
                //stockService.StockExpire(dstLatestStockWithSameExpirationDate);
                stockService.ExpirePreviousStocks(dstLatestStockWithSameExpirationDate, "By Transfer Order " + od.Id);
                //stockService.StockEdit(newSrcStock);
                stockService.InsertStock(newSrcStock, "Stock Created from Transfer Order " + od.Id);
                //stockService.StockEdit(newDstStock);
                stockService.InsertStock(newDstStock, "Stock Created from Transfer Order " + od.Id);
                stockService.CommitChanges();
                stockService.InsertStockChangeHistory(newSrcStock, srcLatestStockWithSameExpirationDate, StockHistoryChangeType.TRANSFER);
                stockService.InsertStockChangeHistory(newDstStock, dstLatestStockWithSameExpirationDate, StockHistoryChangeType.TRANSFER);
                stockService.CommitChanges();

                #region 1) stockService.StockExpire(srcLatestStockWithSameExpirationDate);

                //// Find the all Previous Stock of same expiration date.
                //var srcLatestStockWithSameExpirationDatePreviousStocks = db.Stocks
                //    .Where(s =>
                //        s.ItemId == srcLatestStockWithSameExpirationDate.ItemId &&
                //        s.LocationId == srcLatestStockWithSameExpirationDate.LocationId &&
                //        s.ExpirationDate == srcLatestStockWithSameExpirationDate.ExpirationDate &&
                //        s.stateValue == (int)StockState.CONFIRMED
                //    );

                //// Update latest Previous Stock
                //if (srcLatestStockWithSameExpirationDatePreviousStocks.Any())
                //{
                //    foreach (var stock in srcLatestStockWithSameExpirationDatePreviousStocks.ToList())
                //    {
                //        stock.stateValue = (int)StockState.EXPIRED;
                //        stock.Note += "Expired at " + GongChaDateTimeWrapper.Now.ToString() + " By Transfer Order "+od.Id+". ";
                //        db.Entry(stock).State = EntityState.Modified;
                //    }
                //}
                #endregion

                #region 2) stockService.StockExpire(dstLatestStockWithSameExpirationDate);
                //var dstLatestStockWithSameExpirationDatePreviousStocks = db.Stocks
                //    .Where(s =>
                //        s.ItemId == dstLatestStockWithSameExpirationDate.ItemId &&
                //        s.LocationId == dstLatestStockWithSameExpirationDate.LocationId &&
                //        s.ExpirationDate == dstLatestStockWithSameExpirationDate.ExpirationDate &&
                //        s.stateValue == (int)StockState.CONFIRMED
                //    );

                //// Update latest Previous Stock
                //if (dstLatestStockWithSameExpirationDatePreviousStocks.Any())
                //{
                //    foreach (var stock in dstLatestStockWithSameExpirationDatePreviousStocks.ToList())
                //    {
                //        stock.stateValue = (int)StockState.EXPIRED;
                //        stock.Note += "Expired at " + GongChaDateTimeWrapper.Now.ToString() + " By Transfer Order " + od.Id + ". ";
                //        db.Entry(stock).State = EntityState.Modified;
                //    }
                //}
                #endregion
                // 3) stockService.StockEdit(newSrcStock);
                // 4) stockService.StockEdit(newDstStock);


                od.StatusValue = (int)DetailStatus.ACCEPTED;
                od.Status = DetailStatus.ACCEPTED;
                db.SaveChanges();
                if (db.TransferOrderDetails.Where(
                    detail =>
                        detail.ParentOrderID == od.ParentOrderID
                        )
                        .All(
                        detail =>
                            detail.StatusValue != (int)DetailStatus.PENDING
                        )
                )
                {
                    od.ParentOrder.StatusValue = (int)OrderStatus.Completed;
                }

                db.SaveChanges();
                scope.Complete();
            }
            return RedirectToAction("History", "StockTake", new { ItemId = od.ItemId, LocationId = od.ParentOrder.DestinationLocationId });
        }

        public ActionResult Reject(int TransferOrderDetailId)
        {
            TransferOrderDetail od = db.TransferOrderDetails.Find(TransferOrderDetailId);
            if (od == null)
            {
                return HttpNotFound();
            }
            od.StatusValue = (int)DetailStatus.REJECTED;
            od.Status = DetailStatus.REJECTED;
            db.SaveChanges();
            if (db.TransferOrderDetails.Where(
                    detail =>
                        detail.ParentOrderID == od.ParentOrderID
                        )
                        .All(
                        detail =>
                            detail.StatusValue != (int)DetailStatus.PENDING
                        )
                )
            {
                od.ParentOrder.StatusValue = (int)OrderStatus.Completed;
            }
            return RedirectToAction("History", "StockTake", new { ItemId = od.ItemId, LocationId = od.ParentOrder.DestinationLocationId });
        }
        #endregion

        #region private helpers
        public Stock CloneStock(Stock pStock)
        {
            if (pStock == null)
            {
                return new Stock()
                {
                    //Checker = CurrentMember,
                    MemberId = CurrentMember.Id,
                    Date = GongChaDateTimeWrapper.Now,
                    ExpirationDate = GongChaDateTimeWrapper.Now
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