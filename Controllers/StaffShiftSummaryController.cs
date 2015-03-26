using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using System.Data.Objects.SqlClient;
using System.Net;

namespace GongChaWebApplication.Controllers
{
    public class StaffShiftSummaryController : BaseController
    {

        //
        // GET: /StaffShiftSummary/

        public ActionResult Index()
        {
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(db.MemberProfiles.ToList());
            }

            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                return View(db.MemberProfiles.Where(mp => mp.Member.Store.Id == CurrentMember.Store.Id && mp.BasedStoreId == CurrentMember.Store.Id).ToList());
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return RedirectToAction("Details", new { id = CurrentMember.Id });
            }
            // Find Profile for Current Member
            return View(db.MemberProfiles.Where(mp => mp.MemberId == CurrentMember.Id).ToList());
        }

        //
        // GET: /StaffShiftSummary/Details/5
        // return the shift detail of staff using Member.Id

        /// <summary>
        /// Return the shift detail of staff
        /// </summary>
        /// <param name="id">Member Id</param>
        /// <returns>View of the shift detail</returns>
        /// 
        [Roles(MemberAccessLevels.STAFF, MemberAccessLevels.WAREHOUSE_MANAGER, SuccessfulRedirectMethod = RedirectMethods.INDEX, Order = 1)]
        [Roles(MemberAccessLevels.MANAGER, MemberAccessLevels.DIRECTOR, FailureRedirectMethod = RedirectMethods.SAME_REQUEST, Order = 2)]
        public ActionResult Details(int id = 0)
        {
            var Member = db.Members.Find(id);
            var MemberProfile = db.MemberProfiles.Where(mp => mp.MemberId == id).FirstOrDefault();
            if (Member == null)
            {
                return HttpNotFound();
            }
            if (MemberProfile == null)
            {
                return RedirectToAction("Index", "MemberProfile");
            }

            // Managers can only mange its own store.
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id != Member.Store.Id)
            {
                return RedirectToAction("Index");
            }

            var Shifts = db.Shifts.Where(sh => sh.MemberId == id).OrderBy(sh => sh.StartHour);

            // Get the Available Years
            List<int> availableYears = Shifts.Select(
                    sh => sh.StartHour.Year
                ).Distinct().ToList();

            Dictionary<int, List<int>> availableWeekNumbersPerYears = new Dictionary<int, List<int>>();
            // Get the Available Week Numbers for each year
            foreach (var year in availableYears)
            {
                var shiftOfYear = Shifts.Where(sh => sh.StartHour.Year == year);
                var startOfYear = new DateTime(year, 1, 1);
                var firstMondayOfYear = startOfYear.AddDays(-(startOfYear.DayOfWeek - DayOfWeek.Monday));
                var weekNumbers = shiftOfYear.Select(
                    sh =>
                        (int)SqlFunctions.DateDiff("d", firstMondayOfYear, sh.StartHour) / 7 + 1
                    ).Distinct().OrderByDescending(a => a).ToList();
                availableWeekNumbersPerYears.Add(year, weekNumbers);
            }

            StaffShiftViewModel vm = new StaffShiftViewModel()
            {
                Member = Member,
                AvailableWeekNumbersForYears = availableWeekNumbersPerYears
            };

            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRangeDetail(int memberId, int weekNumber, DateTime startDate, DateTime endDate)
        {
            if (CurrentMember == null)
            {
                return null;
            }
            Member member = db.Members.Find(memberId);
            var memberProfiles = db.MemberProfiles.Where(mp => mp.MemberId == memberId).ToList();
            //MemberProfile memberProfile = null;
            //if (memberProfiles.Any())
            //{
            //    memberProfile = memberProfiles.First();
            //}

            var weekDays = new List<DateTime>();
            if (startDate <= endDate)
            {
                DateTime RunningDate = startDate;
                while (RunningDate < endDate)
                {
                    weekDays.Add(RunningDate);
                    RunningDate = RunningDate.AddDays(1);
                }
            }


            Dictionary<Member, StaffShiftViewModel> staffShiftSummaries = new Dictionary<Member, StaffShiftViewModel>();
            var WageTypes = db.WageTypes.ToList();

            StaffRangeShiftViewModel vm = new StaffRangeShiftViewModel()
            {
                Member = member,
                WeekNumber = weekNumber,
                WeekDays = weekDays,
                WageTypes = WageTypes,
                MemberProfiles = memberProfiles,
                //MemberRates = db.MemberRates.Where(r => r.MemberProfileId == memberProfile.Id).ToList(),
                RatesForProfile = db.MemberRates
                                .Where(r => r.MemberProfile.MemberId == memberId)
                                .GroupBy(r => r.MemberProfile)
                                .ToDictionary(grp => grp.Key, grp => grp.ToList()),
                Shifts = db.Shifts.Where(
                            sh =>
                                sh.MemberId == memberId &&
                                sh.StartHour >= startDate &&
                                sh.StartHour <= endDate
                                ).ToList()
            };
            return PartialView("RangeDetails", vm);
        }
    }
}