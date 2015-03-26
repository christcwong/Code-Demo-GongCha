using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Controllers
{
    public class SalesTargetController : BaseController
    {

        //
        // GET: /SalesTarget/

        /// <summary>
        /// List all stores that are available for Sales Target Setup
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            
            if (!Accessible())
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }

            List<Store> stores = null;

            if (CurrentMember.AccessLevel == "director")
            {
                stores = db.Stores.ToList();
            }
            else if (CurrentMember.AccessLevel == "manager")
            {
                if (CurrentMember.Store != null)
                {
                    stores = new List<Store>() { db.Stores.Find(CurrentMember.Store.Id) };
                }
            }

            if (stores == null)
            {
                return HttpNotFound();
            }
            return View(stores);
        }

        /// <summary>
        /// List all sales targets for selected stores
        /// </summary>
        /// <param name="LocationId"></param>
        /// <returns></returns>
        public ActionResult List(int StoreId = 0)
        {
            var store = db.Stores.Find(StoreId);
            if (store == null)
            {
                return HttpNotFound();
            }
            var targets = db.SalesTargets.Where(t => t.StoreId == StoreId)
                .Include(t => t.Store)
                .Include(t => t.WageType);
            CurrentStore = store;
            ViewBag.Store = store;
            ViewBag.CurrentMember = CurrentMember;
            return View(targets);
        }

        //
        // GET: /SalesTarget/Details/5


        public ActionResult Details(int id = 0)
        {
            SalesTarget salestarget = db.SalesTargets.Find(id);
            if (salestarget == null)
            {
                return HttpNotFound();
            }
            return View(salestarget);
        }

        //
        // GET: /SalesTarget/Create

        public ActionResult Create()
        {
            
            if (!Accessible())
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            if (CurrentMember.AccessLevel == "director")
            {
                ViewBag.StoreId = new SelectList(db.Stores, "Id", "Name");
            }
            else
            {
                ViewBag.StoreId = new SelectList(db.Stores.Where(s => s.Id == CurrentMember.Store.Id), "Id", "Name");
            }

            ViewBag.WageTypeId = new SelectList(db.WageTypes.Where(t=>t.Id!=1), "Id", "Name");
            return View();
        }

        //
        // POST: /SalesTarget/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SalesTarget salestarget)
        {
            if (ModelState.IsValid)
            {
                db.SalesTargets.Add(salestarget);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StoreId = new SelectList(db.Stores, "Id", "Name", salestarget.StoreId);
            ViewBag.WageTypeId = new SelectList(db.WageTypes.Where(t => t.Id != 1), "Id", "Name", salestarget.WageTypeId);
            return View(salestarget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ListCreate()
        {
            
            if (!Accessible())
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            if (CurrentMember.AccessLevel == "director")
            {
                ViewBag.StoreId = new SelectList(db.Stores, "Id", "Name");
            }
            else
            {
                ViewBag.StoreId = new SelectList(db.Stores.Where(s => s.Id == CurrentMember.Store.Id), "Id", "Name");
            }


            var wageTypes = db.WageTypes.Where(t => t.Id != 1).ToList();

            List<SalesTarget> salesTargetForWageType = new List<SalesTarget>();
            foreach (var wageType in wageTypes)
            {
                salesTargetForWageType.Add(new SalesTarget() { WageType = wageType, WageTypeId = wageType.Id });
            }


            SalesTargetListCreateViewModel vm = new SalesTargetListCreateViewModel()
            {
                Store = CurrentStore,
                Date = GongChaDateTimeWrapper.Now.Date,
                WageTypes = db.WageTypes.Where(t => t.Id != 1).ToList(),
                SalesTargetsForWageType = salesTargetForWageType,

            };
            return View(vm);
        }

        //
        // POST: /SalesTarget/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ListCreate(SalesTargetListCreateViewModel vm)
        {
            
            if (!Accessible())
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            if (ModelState.IsValid)
            {
                var store = db.Stores.Find(vm.StoreId);
                foreach (var salesTarget in vm.SalesTargetsForWageType)
                {
                    salesTarget.Store = store;
                    salesTarget.StoreId = store.Id;
                    salesTarget.WageType = db.WageTypes.Find(salesTarget.WageTypeId);
                    salesTarget.Date = vm.Date;
                    db.SalesTargets.Add(salesTarget);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StoreId = new SelectList(db.Stores, "Id", "Name", vm.StoreId);
            return View(vm);
        }


        //
        // GET: /SalesTarget/Edit/5

        public ActionResult Edit(int id = 0)
        {
            SalesTarget salestarget = db.SalesTargets.Find(id);
            if (salestarget == null)
            {
                return HttpNotFound();
            }
            ViewBag.StoreId = new SelectList(db.Stores, "Id", "Name", salestarget.StoreId);
            ViewBag.WageTypeId = new SelectList(db.WageTypes.Where(t => t.Id != 1), "Id", "Name", salestarget.WageTypeId);
            return View(salestarget);
        }

        //
        // POST: /SalesTarget/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SalesTarget salestarget)
        {
            if (ModelState.IsValid)
            {
                db.Entry(salestarget).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StoreId = new SelectList(db.Stores, "Id", "Name", salestarget.StoreId);
            ViewBag.WageTypeId = new SelectList(db.WageTypes.Where(t => t.Id != 1), "Id", "Name", salestarget.WageTypeId);
            return View(salestarget);
        }

        //
        // GET: /SalesTarget/Delete/5

        public ActionResult Delete(int id = 0)
        {
            SalesTarget salestarget = db.SalesTargets.Find(id);
            if (salestarget == null)
            {
                return HttpNotFound();
            }
            return View(salestarget);
        }

        //
        // POST: /SalesTarget/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SalesTarget salestarget = db.SalesTargets.Find(id);
            db.SalesTargets.Remove(salestarget);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}