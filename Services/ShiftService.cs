using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GongChaWebApplication.Models;
using GongChaWebApplication.Controllers;
using System.Data;
using System.Transactions;
using System.Data.Objects.SqlClient;
using AutoMapper;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Services
{
    public class ShiftService : BaseController
    {
        SalesService salesService;
        MemberUnavailabilityService unavailabiltityService;
        public ShiftService(GongChaDbContext db)
        {
            this.db = db;
            this.salesService = new SalesService(this.db);
            this.unavailabiltityService = new MemberUnavailabilityService(this.db);
        }

        /// <summary>
        /// Wrapper Function for Updating wage of a shift
        /// </summary>
        /// <param name="ShiftId"></param>
        /// <returns></returns>
        public Shift ShiftWageUpdate(int ShiftId = 0)
        {
            Shift shift = db.Shifts.Find(ShiftId);
            if (shift == null)
            {
                return null;
            }
            return ShiftWageUpdate(shift);
        }

        public Shift ShiftWageUpdate(Shift shift)
        {
            //if (!Accessible())
            //{
            //    return null;
            //}
            //if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR
            //    || (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && shift.StoreId == CurrentMember.Store.Id))
            //{
            // Even If the shift is already reviewed, salary should be updated.
            //if (shift.ReviewDate.HasValue)
            //{
            //    return shift;
            //}

            // if there is not yet reviewed shift for the member
            if (!shift.ReviewDate.HasValue)
            {
                // 
            }
            // If there is already wage update *after* shift review, then no need to update this shift.
            if (shift.ReviewDate.HasValue && shift.WageUpdateDate.HasValue && shift.ReviewDate.Value < shift.WageUpdateDate.Value)
            {
                return shift;
            }
            // Find the sales and sales target of that day.
            // If both of them exists, proceed to calculate the wage
            #region Check WageType for shift
            var wageType = salesService.GetWageTypeForDay(shift.StoreId, shift.StartHour.Date);
            #endregion

            #region Update Shift

            var memberProfiles = db.MemberProfiles.Where(
                mp =>
                    mp.MemberId == shift.MemberId &&
                    mp.BasedStoreId == shift.StoreId
                    );
            if (!memberProfiles.Any())
            {
                // can't update.
                return shift;
            }
            var memberProfile = memberProfiles.First();
            var memberRateBase = db.MemberRates.Where(r => r.WageTypeId == wageType.Id && r.MemberProfileId == memberProfile.Id).Select(r => r.WageBase);
            if (!memberRateBase.Any())
            {
                // can't update.
                return shift;
            }
            var wageAmount = memberRateBase.First() * shift.TotalHour;
            shift.WageType = wageType;
            shift.WageTypeId = wageType.Id;
            shift.WageAmount = wageAmount;
            shift.WageUpdateDate = GongChaDateTimeWrapper.Now;

            db.Entry(shift).State = EntityState.Modified;
            db.SaveChanges();
            return shift;
            #endregion
            //}
            //return null;
        }

        public Shift ReviewShift(int ShiftId = 0)
        {
            Shift shift = db.Shifts.Find(ShiftId);
            if (shift == null)
            {
                return null;
            }
            return ReviewShift(shift);
        }

        public Shift ReviewShift(Shift shift)
        {
            // Notice these services cannot access to Session data.
            //if (!Accessible())
            //{
            //    return null;
            //}
            //if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR
            //    || (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && shift.StoreId == CurrentMember.Store.Id))
            //{
            if (!shift.ReviewDate.HasValue)
            {
                shift.ReviewDate = GongChaDateTimeWrapper.Now;
                db.Entry(shift).State = EntityState.Modified;
                db.SaveChanges();
            }
            return shift;
            //}
            //return null;
        }

        public double GetTotalManHour(int StoreId, DateTime Date)
        {
            var shifts =
             db.Shifts.Where(s => s.StoreId == StoreId &&
                    SqlFunctions.DatePart("year", s.StartHour).Value == Date.Year &&
                    SqlFunctions.DatePart("month", s.StartHour).Value == Date.Month &&
                    SqlFunctions.DatePart("day", s.StartHour).Value == Date.Day);
            if (shifts.Any())
            {
                return shifts.Select(s => s.TotalHour).Sum();
            }
            return 0;
        }

        public void UpdateSalesTarget(int StoreId, DateTime Date)
        {
            // Get the total hours of shift for store on that day.
            // then for each wage type, update/create salestarget
            var totalManHour = GetTotalManHour(StoreId, Date);
            if (totalManHour == 0)
            {
                // Meaningless to update.
                return;
            }

            var store = db.Stores.Find(StoreId);

            var existingSalesTargets = db.SalesTargets.Where(t => t.StoreId == StoreId &&
                    SqlFunctions.DatePart("year", t.Date).Value == Date.Year &&
                    SqlFunctions.DatePart("month", t.Date).Value == Date.Month &&
                    SqlFunctions.DatePart("day", t.Date).Value == Date.Day);

            var wageTypes = db.WageTypes.Where(wt => wt.Id != 1).ToList();
            foreach (var type in wageTypes)
            {
                //salesTargetsForRange.Add(weekDay, new SalesTarget() { Date = weekDay, Note = "Generated", StoreId = storeId, Target = type.targetMultiplier, WageTypeId = type.Id, WageType = type, Store = store });
                var existingTargetOfType = existingSalesTargets.Where(t => t.WageTypeId == type.Id);
                var newTarget = type.targetMultiplier * totalManHour;
                if (existingTargetOfType.Any())
                {
                    // update
                    foreach (var target in existingTargetOfType)
                    {
                        target.Target = newTarget;
                        db.Entry(target).State = EntityState.Modified;
                    }
                }
                else
                {
                    // create
                    var newSalesTarget = new SalesTarget()
                    {
                        Date = Date,
                        Note = "Generated",
                        StoreId = StoreId,
                        Target = newTarget,
                        WageTypeId = type.Id,
                        WageType = type,
                        Store = store
                    };
                    db.SalesTargets.Add(newSalesTarget);
                }
            }
            db.SaveChanges();
        }

        public ShiftPeriodViewModel GetShiftPeriodViewModel(int StoreId, DateTime StartTime, DateTime EndTime)
        {
            var StartDate = StartTime.Date;
            var EndDate = EndTime.Date.AddDays(1).AddMilliseconds(-1);
            // gather related data
            var store = db.Stores.Find(StoreId);
            var members = db.MemberProfiles
                .Where(
                    p =>
                        p.BasedStoreId == StoreId
                        && p.Member.stateValue == (int)MemberState.ACTIVE
                        )
                .Select(p => p.Member)
                .OrderBy(a => (a.Store.Id == StoreId ? 1 : 2));
            var memberIds = members.Select(m => m.Id);
            var shifts = db.Shifts.Where(
                    s =>
                        memberIds.Contains(s.MemberId) &&
                        s.StartHour >= StartDate &&
                        s.StartHour <= EndDate
                ).OrderBy(s => s.MemberId).ThenBy(s => s.StartHour).ToList();

            // This things is damn troublesome.
            //var unavailabilities = db.MemberUnavailabilities.Where(
            //        u =>
            //            memberIds.Contains(u.MemberId) &&
            //            u.StartHour >= StartDate &&
            //            u.StartHour <= EndDate
            //    );
            var unavailabilities = unavailabiltityService.GetMemberUnavailabilitiesForRange(memberIds, StartDate, EndDate);

            var days = new List<DateTime>();
            if (StartDate <= EndDate)
            {
                var RunningTime = StartDate;
                while (RunningTime < EndDate)
                {
                    days.Add(RunningTime);
                    RunningTime = RunningTime.AddDays(1);
                }
            }

            // Data Massage
            Dictionary<Member, ShiftPeriodMemberViewModel> memberDetails = new Dictionary<Member, ShiftPeriodMemberViewModel>();
            foreach (var member in members)
            {
                // create shift period member view model
                var shiftsForMember = shifts.Where(s => s.MemberId == member.Id).ToLookup(item => item.StartHour.Date);
                var unavailabilitiesForMember = unavailabilities.Where(u => u.MemberId == member.Id).ToLookup(item => item.StartHour.Value.Date);
                var dayDetails = new Dictionary<DateTime, ShiftPeriodMemberDayViewModel>();
                foreach (var day in days)
                {
                    dayDetails.Add(day,
                        new ShiftPeriodMemberDayViewModel
                        {
                            Member = member,
                            MemberId = member.Id,
                            Date = day,
                            Shifts = shiftsForMember[day].ToList().Select(
                                        (shift, index) =>
                                            new ShiftEditViewModel()
                                            {
                                                rowId = index,
                                                Shift = shift,
                                                Date = shift.StartHour.Date,
                                                MarkDeleted = false,
                                            }
                                    ).ToList(),
                            MemberUnavailabilities = unavailabilitiesForMember[day].ToList()
                        });
                }
                // ok there is a member...
                ShiftPeriodMemberViewModel model = new ShiftPeriodMemberViewModel()
                {
                    Member = member,
                    MemberId = member.Id,
                    DayDetails = dayDetails,
                };
                memberDetails.Add(member, model);
            }


            // Return results

            ShiftPeriodViewModel vm = new ShiftPeriodViewModel
            {
                LocationId = store.LocationId,
                Location = store.Location,
                StartDate = StartDate,
                EndDate = EndDate,
                Days = days,
                Members = members.Where(m => m.Store.Id == StoreId).ToList(),
                BorrowedMembers = members.Where(m => m.Store.Id != StoreId).ToList(),
                //Shifts = shifts.ToList(),
                //MemberUnavailabilities = unavailabilities.ToList(),
                Store = store,
                StoreId = store.Id,
                WageTypes = db.WageTypes.ToList(),
                MemberDetails = memberDetails,
                NumberOfWeeksToView = 5,
            };
            return vm;
        }

        #region shift search-update-create
        public List<Shift> GetShifts(int MemberId, DateTime StartTime, DateTime EndTime)
        {
            var shifts = db.Shifts.Where(
                    s =>
                        s.MemberId == MemberId &&
                        s.EndHour > StartTime &&
                        s.StartHour < EndTime
                ).ToList();

            return shifts;
        }

        public Shift GetShifts(int MemberId, int StoreId, DateTime StartTime, DateTime EndTime)
        {
            var shifts = db.Shifts.Where(
                    s =>
                        s.MemberId == MemberId &&
                        s.StoreId == StoreId &&
                        s.EndHour > StartTime &&
                        s.StartHour < EndTime
                ).ToList();

            if (shifts.Any())
            {
                return shifts.First();
            }
            return null;
        }

        public Shift UpdateOrCreateShift(ShiftEditViewModel vm)
        {
            // check if the shift in vm is present
            if (vm.Shift != null)
            {
                DateTime? latestReviewTime = GetLatestShiftReviewedTimeForStore(vm.Shift.StoreId);
                DateTime? EndOfWeekOfLatestReviewTime = (latestReviewTime.HasValue ? GongChaDateTimeWrapper.EndOfWeek(latestReviewTime.Value) : (DateTime?)null);

                // check if there is an existing shift.

                var shift = db.Shifts.Find(vm.Shift.Id);

                if (shift == null)
                {
                    // if no existing shfit,
                    // create shift using the shift in vm
                    if (EndOfWeekOfLatestReviewTime.HasValue && vm.Shift.StartHour.Date <= EndOfWeekOfLatestReviewTime.Value)
                    {
                        throw new ArgumentException("Shifts cannot be created After Review is made for the week");
                    }

                    if (vm.MarkDeleted == false)
                    {
                        // Need to check if there is timeslot collision...
                        if (unavailabiltityService.IsAvailable(vm.Shift.MemberId, vm.Shift.StartHour, vm.Shift.EndHour))
                        {
                            // and also need to check whether member already has a shift (except this one)
                            // in the mentioned time.
                            // in this case "this one" is not yet in db
                            var collidingShifts = GetShifts(vm.Shift.MemberId, vm.Shift.StartHour, vm.Shift.EndHour);
                            if (collidingShifts.Any())
                            {
                                throw new ArgumentException("This member is having a shift at "
                                    + collidingShifts.First().StartHour.ToString("HH:mm")
                                    + " - "
                                    + collidingShifts.First().EndHour.ToString("HH:mm")
                                    + " at "
                                    + collidingShifts.First().Store.Name);
                            }
                            var returnShift = db.Shifts.Add(vm.Shift);
                            db.SaveChanges();
                            return returnShift;
                        }
                        else
                        {
                            throw new ArgumentException("Member is not available at " + vm.Shift.StartHour.ToString("HH:mm") + " - " + vm.Shift.EndHour.ToString("HH:mm"));
                        }
                    }
                    // Mark deleted record without db record => no action is needed
                    return null;
                }
                else
                {
                    if (EndOfWeekOfLatestReviewTime.HasValue && vm.Shift.StartHour.Date <= EndOfWeekOfLatestReviewTime.Value)
                    {
                        throw new ArgumentException("Shifts cannot be modified After Review is made for the week");
                    }
                    if (vm.MarkDeleted == false)
                    {
                        // Mapping ....
                        var ShiftClone = Mapper.CreateMap<Shift, Shift>();
                        ShiftClone.ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
                        ShiftClone.ForMember(dst => dst.Id, opt => opt.Ignore());
                        shift = Mapper.Map(vm.Shift, shift);
                        db.Entry(shift).State = EntityState.Modified;
                        db.SaveChanges();
                        return shift;
                    }
                    else
                    {
                        // marked deleted... that means we need to update the shifts...
                        db.Shifts.Remove(shift);
                        db.SaveChanges();
                        return null;
                    }
                }
            }
            else
            {
                throw new ArgumentException("Shift Edit View Model Inputted does not has a Shift. (vm.shift is empty)");
            }
        }

        private DateTime? GetLatestShiftReviewedTimeForStore(int StoreId)
        {
            return db.Shifts.Where(s =>
                    s.StoreId == StoreId &&
                    s.ReviewDate != null
                ).Max(s => s.StartHour);
        }
        #endregion
    }
}