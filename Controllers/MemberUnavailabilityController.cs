using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using GongChaWebApplication.Services;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Controllers
{
    public class MemberUnavailabilityController : BaseController
    {
        private MemberUnavailabilityService unavailabilityService;
        public static List<String> ReccurenceList = new List<String>() { "Day(s)", "Week(s)", "Month(s)" };
        //
        // GET: /MemberUnavailability/

        public MemberUnavailabilityController()
            : base()
        {
            this.unavailabilityService = new MemberUnavailabilityService(this.db);
        }

        // List all stores 
        // If current member is a staff, redirect to his own unavailablility time
        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return RedirectToAction("ListMember", new { id = CurrentMember.Id });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                return RedirectToAction("List", new { id = CurrentMember.Store.Id });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(db.Stores.Select(s => s.Location).ToList());
            }
            return View();
        }

        /// <summary>
        /// list all members of a store of Location Id Id.
        /// </summary>
        /// <param name="id">Location Id of Store</param>
        /// <returns></returns>
        public ActionResult List(int id)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            var stores = db.Stores.Where(s => s.LocationId == id);
            if (!stores.Any())
            {
                return RedirectToAction("Index");
            }
            var store = stores.First();
            ViewBag.Store = stores.First();
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return RedirectToAction("ListMember", new { id = CurrentMember.Id });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                if (CurrentMember.Store.LocationId == id)
                {
                    return View(db.Members.Where(m => m.Store.Id == store.Id));
                }
                else
                {
                    return RedirectToAction("List", new { id = CurrentMember.Store.LocationId });
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(db.Members.Where(m => m.Store.Id == store.Id));
            }
            return View();
        }

        /// <summary>
        /// List the Unavailability of Staff with MemberId id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ListMember(int id)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            var Member = db.Members.Find(id);
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                ViewBag.Member = CurrentMember;
                return View(db.MemberUnavailabilities.Where(m => m.Memeber.Id == CurrentMember.Id)
                    .OrderByDescending(u => u.StartHour)
                    .ToList());
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                if (Member.Store.Id == CurrentMember.Store.Id)
                {
                    ViewBag.Member = Member;
                    return View(db.MemberUnavailabilities.Where(m => m.Memeber.Id == Member.Id)
                        .OrderByDescending(u => u.StartHour)
                        .ToList());
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                ViewBag.Member = Member;
                return View(db.MemberUnavailabilities.Where(m => m.Memeber.Id == Member.Id)
                    .OrderByDescending(u => u.StartHour)
                    .ToList());
            }
            return RedirectToAction("Index");
        }

        //
        // GET: /MemberUnavailability/Details/5

        public ActionResult Details(int id = 0)
        {
            MemberUnavailability memberunavailability = db.MemberUnavailabilities.Find(id);
            if (memberunavailability == null)
            {
                return HttpNotFound();
            }
            return View(memberunavailability);
        }

        //
        // GET: /MemberUnavailability/Create

        public ActionResult Create(int MemberId)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            var member = db.Members.Find(MemberId);
            ViewBag.MemberId = MemberId;
            ViewBag.Member = member;
            ViewBag.RecurrencePeriodUnit = new SelectList(ReccurenceList);
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                if (MemberId == CurrentMember.Id)
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Create", new { MemberId = CurrentMember.Id });
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                if (member.Store.Id == CurrentMember.Store.Id)
                {
                    return View();
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View();
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /MemberUnavailability/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MemberUnavailability memberunavailability)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            var member = db.Members.Find(memberunavailability.MemberId);
            ViewBag.MemberId = memberunavailability.MemberId;
            ViewBag.Member = member;
            ViewBag.RecurrencePeriodUnit = new SelectList(ReccurenceList);
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                if (member.Id != CurrentMember.Id)
                {
                    ViewBag.StatusMessage = "You are not authorized to modifiy it for other users";
                    return View(memberunavailability);
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                if (member.Store.Id != CurrentMember.Store.Id)
                {
                    ViewBag.StatusMessage = "You are not authorized to modifiy it for staff of other stores";
                    return View(memberunavailability);
                }
            }
            if (memberunavailability.EndHour == null || memberunavailability.StartHour == null)
            {
                ViewBag.StatusMessage = "Fill in the start time or end time";
                return View(memberunavailability);
            }

            if (ModelState.IsValid)
            {
                db.MemberUnavailabilities.Add(memberunavailability);
                db.SaveChanges();
                return RedirectToAction("ListMember", new { id = memberunavailability.MemberId });
            }
            ViewBag.StatusMessage = "System Error";
            return View(memberunavailability);
        }

        //
        // GET: /MemberUnavailability/Edit/5

        public ActionResult Edit(int id = 0)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            MemberUnavailability memberunavailability = db.MemberUnavailabilities.Find(id);
            if (memberunavailability == null)
            {
                return HttpNotFound();
            }
            var member = db.Members.Find(memberunavailability.MemberId);
            ViewBag.MemberId = memberunavailability.MemberId;
            ViewBag.Member = member;
            ViewBag.RecurrencePeriodUnit = new SelectList(ReccurenceList, memberunavailability.RecurrencePeriodUnit);
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                if (memberunavailability.MemberId == CurrentMember.Id)
                {
                    return View(memberunavailability);
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                if (member.Store.Id == CurrentMember.Store.Id)
                {
                    return View(memberunavailability);
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(memberunavailability);
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /MemberUnavailability/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MemberUnavailability memberunavailability)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            var member = db.Members.Find(memberunavailability.MemberId);
            ViewBag.MemberId = memberunavailability.MemberId;
            ViewBag.Member = member;
            ViewBag.RecurrencePeriodUnit = new SelectList(ReccurenceList, memberunavailability.RecurrencePeriodUnit);
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                if (member.Id != CurrentMember.Id)
                {
                    ViewBag.StatusMessage = "You are not authorized to modifiy it for other users";
                    return View(memberunavailability);
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                if (member.Store.Id != CurrentMember.Store.Id)
                {
                    ViewBag.StatusMessage = "You are not authorized to modifiy it for staff of other stores";
                    return View(memberunavailability);
                }
            }
            if (memberunavailability.StartHour == null || memberunavailability.EndHour == null)
            {
                ViewBag.StatusMessage = "Fill in the start time or end time";
                return View(memberunavailability);
            }

            if (ModelState.IsValid)
            {
                db.Entry(memberunavailability).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListMember", new { id = memberunavailability.MemberId });
            }
            ViewBag.StatusMessage = "System Error";
            return View(memberunavailability);
        }

        //
        // GET: /MemberUnavailability/Delete/5

        public ActionResult Delete(int id = 0)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }

            MemberUnavailability memberunavailability = db.MemberUnavailabilities.Find(id);
            if (memberunavailability == null)
            {
                return HttpNotFound();
            }
            return View(memberunavailability);
        }

        //
        // POST: /MemberUnavailability/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }
            MemberUnavailability memberunavailability = db.MemberUnavailabilities.Find(id);
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id == memberunavailability.Memeber.Store.Id) ||
                ((CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER) && memberunavailability.MemberId == CurrentMember.Id)
                )
            {

                db.MemberUnavailabilities.Remove(memberunavailability);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetMemberUnavailabilities(int MemberId)
        {
            if (CurrentMember == null)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                if (MemberId != CurrentMember.Id)
                {
                    return Json(new { }, JsonRequestBehavior.AllowGet);
                }
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                var member = db.Members.Find(MemberId);
                if (member != null && member.Store.Id != CurrentMember.Store.Id)
                {
                    return Json(new { }, JsonRequestBehavior.AllowGet);
                }
            }
            var currentTime = GongChaDateTimeWrapper.Now;
            var allPossibleRecords = unavailabilityService.GetMemberUnavailabilitiesForRange(MemberId, GongChaDateTimeWrapper.Now, GongChaDateTimeWrapper.Now.AddDays(7));
            //recordAfterCurrentTime.ToList().Union(recurrenceRecordAfterCurrentTime).OrderBy(u => u.StartHour);
            return PartialView(allPossibleRecords);
            //return Json(allPossibleRecords, JsonRequestBehavior.AllowGet);
        }
    }
}