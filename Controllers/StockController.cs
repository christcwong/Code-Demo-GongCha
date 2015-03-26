using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using Newtonsoft.Json;
using GongChaWebApplication.Helpers;
using GongChaWebApplication.Services;

namespace GongChaWebApplication.Controllers
{
    [Authorize]
    public class StockController : BaseController
    {

        StockService service;

        public StockController():base()
        {
            this.service = new StockService(this.db);
        }

        

        public ActionResult GetLatestStock(int LocationId, int ItemId)
        {
            var stocks = service.GetActiveStocks(LocationId, ItemId);
            StockViewModel vm = null;
            if (stocks.Any())
            {
                var latestStock = stocks.Include(s => s.Item).Include(s => s.Location).Include(s => s.Item.PackageUnit).First();
                vm = new StockViewModel()
                {
                    Date = latestStock.Date,
                    ExpirationDate = latestStock.ExpirationDate,
                    Id = latestStock.Id,
                    Item = latestStock.Item,
                    ItemId = latestStock.ItemId,
                    Location = latestStock.Location,
                    LocationId = latestStock.LocationId,
                    Quantity = latestStock.Quantity
                };
            }
            else
            {
                vm = new StockViewModel()
                {
                    ItemId = ItemId,
                    Item = db.Items.Find(ItemId),
                    LocationId = LocationId,
                    Quantity = 0
                };
            }
            return Json(vm, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetLatestStocks(int LocationId, int ItemId)
        {
            var stocks = service.GetActiveStocks(LocationId, ItemId);
            Item item = db.Items.Find(ItemId);
            List<StockViewModel> vm = new List<StockViewModel>();
            if (stocks.Any())
            {
                foreach (var stock in stocks.ToList())
                {
                    vm.Add(new StockViewModel()
                    {
                        Date = stock.Date,
                        ExpirationDate = stock.ExpirationDate,
                        Id = stock.Id,
                        Item = stock.Item,
                        ItemId = stock.ItemId,
                        Location = stock.Location,
                        LocationId = stock.LocationId,
                        Quantity = stock.Quantity
                    });
                }

            }
            else
            {
                vm.Add(new StockViewModel()
                {
                    ItemId = ItemId,
                    Item = item,
                    LocationId = LocationId,
                    Quantity = 0
                });
            }
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        public List<Stock> GetHistoryForItem(int ItemId = 0, int LocationId = 0)
        {
            CurrentLocation = db.Locations.Where(loc => loc.Id == LocationId).FirstOrDefault();
            if (!Accessible())
            {
                CurrentLocation = null;
                return new List<Stock>();
            }
            var stocks = service.GetActiveStocks(LocationId, ItemId);
            return stocks.ToList();
        }




        //public ActionResult History(int ItemId = 0, int LocationId = 0)
        //{
        //    CurrentLocation = db.Locations.Where(loc => loc.Id == LocationId).FirstOrDefault();
        //    if (!Accessible())
        //    {
        //        CurrentLocation = null;
        //        return RedirectToAction("Logout", "Member");
        //    }
        //    var stocks = GetActiveStocks(LocationId,ItemId);
        //    if (stocks.Any())
        //    {
        //        PrepareViewBag();
        //        return View(stocks.Include(s => s.Item).Include(s => s.Location).Include(s => s.Checker).ToList());
        //    }
        //    return HttpNotFound();
        //}

        ////
        //// GET: /Stock/Create

        //public ActionResult Create()
        //{
        //    if (!Accessible())
        //    {
        //        CurrentLocation = null;
        //        return RedirectToAction("Logout", "Member");
        //    }
        //    PrepareViewBag();
        //    Stock stock = new Stock
        //    {
        //        Date = GongChaDateTimeWrapper.Now,
        //        ExpirationDate = GongChaDateTimeWrapper.Now,
        //        //Checker = CurrentMember,
        //        MemberId = CurrentMember.Id
        //    };
        //    if (CurrentLocation != null)
        //    {
        //        stock.LocationId = CurrentLocation.Id;
        //        //stock.Location = CurrentStore.Location;
        //    }
        //    return View(stock);
        //}



        ////
        //// POST: /Stock/Create

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Stock stock)
        //{
        //    if (!Accessible())
        //    {
        //        CurrentLocation = null;
        //        return RedirectToAction("Logout", "Member");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        db.Stocks.Add(stock);
        //        db.SaveChanges();
        //        return RedirectToAction("List", new { Id = CurrentLocation.Id });
        //    }
        //    PrepareViewBag();
        //    return View(stock);
        //}

        ////
        //// GET: /Stock/Edit/5

        //public ActionResult Edit(int id = 0)
        //{
        //    if (!Accessible())
        //    {
        //        CurrentLocation = null;
        //        return RedirectToAction("Logout", "Member");
        //    }
        //    Stock stock = db.Stocks.Find(id);
        //    if (stock == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    PrepareViewBag();
        //    return View(stock);
        //}

        ////
        //// POST: /Stock/Edit/5

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Stock stock)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(stock).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("List", new { Id = CurrentLocation.Id });
        //    }
        //    PrepareViewBag();
        //    return View(stock);
        //}

        //////
        ////// GET: /Stock/Delete/5

        ////public ActionResult Delete(int id = 0)
        ////{
        ////    Stock stock = db.Stocks.Find(id);
        ////    if (stock == null)
        ////    {
        ////        return HttpNotFound();
        ////    }
        ////    return View(stock);
        ////}

        //////
        ////// POST: /Stock/Delete/5

        ////[HttpPost, ActionName("Delete")]
        ////[ValidateAntiForgeryToken]
        ////public ActionResult DeleteConfirmed(int id)
        ////{
        ////    Stock stock = db.Stocks.Find(id);
        ////    db.Stocks.Remove(stock);
        ////    db.SaveChanges();
        ////    return RedirectToAction("Index");
        ////}

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}

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
    }
}