using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using System.Data.Objects.SqlClient;
using GongChaWebApplication.Services;
using System.Net;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Controllers
{
    public class StoreShiftSummaryController : BaseController
    {
        private ShiftService shiftService;
        private SalesService salesService;
        //
        // GET: /StoreShift/
        public StoreShiftSummaryController()
            : base()
        {
            this.shiftService = new ShiftService(this.db);
            this.salesService = new SalesService(this.db);
        }

        public ActionResult Index()
        {
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(db.Stores.ToList());
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                return View(db.Stores.ToList());
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF)
            {
                return RedirectToAction("Details", new { id = CurrentMember.Store.Id });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return RedirectToAction("Details", new { id = CurrentMember.Store.Id });
            }

            return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
        }

        //
        // GET: /StoreShift/Details/5

        /// <summary>
        /// Get the store shift history for store
        /// </summary>
        /// <param name="id">Id of Store</param>
        /// <returns>View of store shifts</returns>
        public ActionResult Details(int id = 0)
        {

            Store store = db.Stores.Find(id);
            CurrentLocation = store.Location;
            if ((CurrentMember.AccessLevel == MemberAccessLevels.STAFF ||
                CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER) &&
                CurrentMember.Store.Id != id)
            {
                return RedirectToAction("Details", new { id = CurrentMember.Store.Id });
            }
            Log("Validated");
            // Find all shifts for that store
            var members = db.MemberProfiles.Where(mp => mp.BasedStoreId == id).Select(mp => mp.Member);
            var memberIdList = members.Select(m => m.Id).ToList();
            var shifts = db.Shifts.Where(sh =>
                            sh.StoreId == id
                //&& memberIdList.Contains(sh.MemberId)
                         );


            // Get the Available Years
            List<int> availableYears = shifts.Select(
                    sh => sh.StartHour
                )
                .ToList()
                .Select(startHour=>GongChaDateTimeWrapper.Year(startHour))
                .Distinct().ToList();

            Dictionary<int, List<int>> availableWeekNumbersPerYears = new Dictionary<int, List<int>>();
            Dictionary<int, Dictionary<int, DateTime>> StartDateOfWeekNbOfYr = new Dictionary<int, Dictionary<int, DateTime>>();
            Dictionary<int, Dictionary<int, DateTime>> EndDateOfWeekNbOfYr = new Dictionary<int, Dictionary<int, DateTime>>();
            // Get the Available Week Numbers for each year
            foreach (var year in availableYears)
            {
                var shiftOfYear = shifts.Where(sh => sh.StartHour.Year == year);
                //var startOfYear = new DateTime(year, 1, 1);
                //var firstMondayOfYear = startOfYear.AddDays(-(startOfYear.DayOfWeek - DayOfWeek.Monday));

                //var weekNumbers = shiftOfYear.Select(
                //    sh =>
                //        (int)SqlFunctions.DateDiff("d", firstMondayOfYear, sh.StartHour) / 7 + 1
                //    ).Distinct().OrderByDescending(a => a).ToList();

                var weekNumbers = shiftOfYear
                    .Select(s => s.StartHour)
                    .ToList()
                    .Select(startHour => GongChaDateTimeWrapper.WeekNumber(startHour))
                    .Distinct()
                    .OrderByDescending(a => a).
                    ToList();

                availableWeekNumbersPerYears.Add(year, weekNumbers);

                var startDictionaryForWk = weekNumbers.ToDictionary(
                    x => x,
                    x => GongChaDateTimeWrapper.StartOfWeek(year, x)
                );
                var endDictionaryForWk = weekNumbers.ToDictionary(
                    x => x,
                    x => GongChaDateTimeWrapper.EndOfWeek(year, x)
                );
                StartDateOfWeekNbOfYr.Add(year, startDictionaryForWk);
                EndDateOfWeekNbOfYr.Add(year, endDictionaryForWk);
            }

            // stuffing to avoid null values.
            var CurrentYear = GongChaDateTimeWrapper.Now.Year;
            if (!availableWeekNumbersPerYears.Keys.Contains(CurrentYear))
            {
                var CurrentWeekNb = GongChaDateTimeWrapper.WeekNumber(GongChaDateTimeWrapper.Now);
                availableWeekNumbersPerYears.Add(CurrentYear, new List<int> { CurrentWeekNb });
                StartDateOfWeekNbOfYr.Add(CurrentYear, new Dictionary<int, DateTime> { { CurrentWeekNb, GongChaDateTimeWrapper.StartOfWeek(CurrentYear, CurrentWeekNb) } });
                EndDateOfWeekNbOfYr.Add(CurrentYear, new Dictionary<int, DateTime> { { CurrentWeekNb, GongChaDateTimeWrapper.EndOfWeek(CurrentYear, CurrentWeekNb) } });
            }

            StoreShiftViewModel vm = new StoreShiftViewModel
            {
                Store = store,
                AvailableWeekNumbersForYears = availableWeekNumbersPerYears,
                StartDateOfWeekNbOfYr = StartDateOfWeekNbOfYr,
                EndDateOfWeekNbOfYr = EndDateOfWeekNbOfYr
            };

            return View(vm);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRangeDetail(int storeId, int weekNumber, DateTime startDate, DateTime endDate)
        {
            if (CurrentMember == null)
            {
                return PartialView(new StoreRangeShiftViewModel());
            }


            Store store = db.Stores.Find(storeId);

            var startOfThisWeek = GongChaDateTimeWrapper.StartOfWeek(GongChaDateTimeWrapper.Now);
            var weekDays = new List<DateTime>();

            if (startDate <= endDate)
            {
                DateTime RunningDate = startDate;
                while (RunningDate <= endDate)
                {
                    weekDays.Add(RunningDate);
                    RunningDate = RunningDate.AddDays(1);
                }
            }

            foreach (var weekDay in weekDays)
            {
                shiftService.UpdateSalesTarget(storeId, weekDay);
            }


            Dictionary<DateTime, WageType> wageTypeForDay = new Dictionary<DateTime, WageType>();
            foreach (var weekDay in weekDays)
            {
                wageTypeForDay.Add(weekDay, salesService.GetWageTypeForDay(storeId, weekDay));
            }


            // Retrive Sales And Sales Targets
            var salesForRange = db.SalesItems.Where(s => s.Store.Id == storeId && s.Date >= weekDays.Min() && s.Date <= weekDays.Max()).ToDictionary(
                    x => x.Date.Date,
                    x => x
                );

            var salesTargetsForRange = db.SalesTargets.Where(
                            s =>
                                s.Store.Id == storeId &&
                                s.Date >= weekDays.Min() &&
                                s.Date <= weekDays.Max()
                            ).GroupBy(g => g.Date).ToDictionary(
                                grp => grp.Key.Date,
                                grp => grp.ToList()
                            );

            // Retrive all members.
            // Notice the based store id is used when creating shift for member.....
            //var members = db.MemberProfiles.Where(mp => mp.BasedStoreId == storeId).Select(mp => mp.Member).ToList();
            // shifts indeed record whether a member has a shift for that time.
            var members = db.Shifts.Where(
                s =>
                    s.StoreId == storeId
                    && s.StartHour >= weekDays.Min()
                    && s.StartHour <= weekDays.Max()
                    && s.Memeber.Store.Id == storeId
                )
                .Select(s => s.Memeber)
                .Distinct()
                .OrderBy(a => a.AccessLevel)
                .ThenBy(a => a.Id)
                .ToList();

            var borrowedMembers = db.Shifts.Where(
                s =>
                    s.StoreId == storeId
                    && s.StartHour >= weekDays.Min()
                    && s.StartHour <= weekDays.Max()
                    && s.Memeber.Store.Id != storeId
                )
                .Select(s => s.Memeber)
                .OrderBy(a => a.AccessLevel)
                .ThenBy(a => a.Id).ToList();

            // Construct StaffShiftSummaries for display
            Dictionary<Member, StaffRangeShiftViewModel> staffShiftSummaries = new Dictionary<Member, StaffRangeShiftViewModel>();
            var WageTypes = db.WageTypes.ToList();
            bool reviewed = true;
            foreach (var member in members.Union(borrowedMembers))
            {
                var MemberProfile = db.MemberProfiles.Where(mp => mp.MemberId == member.Id && mp.BasedStoreId == storeId).First();
                var MemberRates = db.MemberRates.Where(r => r.MemberProfileId == MemberProfile.Id).ToList();
                var Shifts = db.Shifts.Where(
                    sh =>
                        sh.MemberId == member.Id &&
                        sh.StartHour >= startDate.Date &&
                        sh.StartHour <= endDate.Date
                    ).OrderBy(sh => sh.StartHour).ToList();
                foreach (var shift in Shifts)
                {
                    shiftService.ShiftWageUpdate(shift.Id);
                }
                Shifts = db.Shifts.Where(
                    sh =>
                        sh.MemberId == member.Id &&
                        sh.StartHour >= startDate.Date &&
                        sh.StartHour <= endDate.Date
                    ).OrderBy(sh => sh.StartHour).ToList();
                if (Shifts.Any(s => !s.ReviewDate.HasValue))
                {
                    reviewed = false;
                }
                StaffRangeShiftViewModel model = new StaffRangeShiftViewModel()
                {
                    Member = member,
                    //MemberProfile = MemberProfile,
                    MemberRates = MemberRates,
                    Shifts = Shifts,
                    WageTypes = WageTypes,
                };
                staffShiftSummaries.Add(member, model);
            }


            StoreRangeShiftViewModel vm = new StoreRangeShiftViewModel()
            {
                Store = store,
                Viewer = CurrentMember,
                Members = members,
                BorrowedMembers = borrowedMembers,
                WeekNumber = weekNumber,
                WeekDays = weekDays,
                WageTypes = WageTypes,
                SalesForDay = salesForRange,
                SalesTargetForDay = salesTargetsForRange,
                StaffShiftSummaries = staffShiftSummaries,
                WageTypeForDay = wageTypeForDay,
                Reviewed = reviewed,
            };

            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id == storeId))
            {
                return PartialView("RangeDetails", vm);
            }
            // Managers can only mange its own store.
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id != storeId))
            {
                return PartialView("RangeShifts", vm);
            }
            return PartialView("RangeShifts", new StoreRangeShiftViewModel());
        }

        [HttpPost]
        public ActionResult Review(int storeId, DateTime startDate, DateTime endDate)
        {
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id == storeId))
            {
                // Find all shifts in that period of time
                var Shifts = db.Shifts.Where(
                        sh =>
                            sh.StoreId == storeId &&
                            sh.StartHour >= startDate.Date &&
                            sh.StartHour <= endDate.Date
                        ).OrderBy(sh => sh.StartHour).ToList();

                foreach (var shift in Shifts)
                {
                    shiftService.ReviewShift(shift.Id);
                }
                return Content("Review: Confirmed");
            }
            return Content("Review: You are not authorized to review shifts.");
        }
    }
}