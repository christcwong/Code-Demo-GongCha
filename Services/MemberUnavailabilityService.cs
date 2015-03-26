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
    public class MemberUnavailabilityService : BaseController
    {
        public MemberUnavailabilityService(GongChaDbContext db)
        {
            this.db = db;
        }

        public Boolean IsAvailable(int MemberId, DateTime Date)
        {
            return !IsUnavailable(MemberId, Date);
        }
        public Boolean IsAvailable(int MemberId, DateTime StartTime, DateTime EndTime)
        {
            return !IsUnavailable(MemberId, StartTime, EndTime);
        }
        public Boolean IsUnavailable(int MemberId, DateTime Date)
        {
            // Notice the EndTime is set to the start of the same date... 
            // the over-loaded function will handle the change of time instead of passing a date with Date.Date.AddDays(1)..
            // otherwise they will push to one more day after.
            return IsUnavailable(MemberId, Date.Date, Date.Date);
        }

        public Boolean IsUnavailable(int MemberId, DateTime StartTime, DateTime EndTime)
        {
            return GetMemberUnavailabilitiesForRange(MemberId, StartTime, EndTime).Any();
            //var heuristicfilter = StartTime.AddYears(-3);

            //// if there is any unavailbility at that day, return false
            //Boolean hasUnavailableRecordForDate = db.MemberUnavailabilities.Where(
            //    u =>
            //        u.MemberId == MemberId &&
            //        StartTime <= u.EndHour &&
            //        u.StartHour <= EndTime)
            //        .Any();
            //if (hasUnavailableRecordForDate)
            //{
            //    return true;
            //}
            //// try to calculate recurrence record
            //var recurrenceRecords = db.MemberUnavailabilities.Where(
            //    u =>
            //        u.MemberId == MemberId &&
            //        u.IsRecurrence == true &&
            //        u.RecurrenceCount > 0 &&
            //        u.StartHour <= EndTime && // if the record is behind the date time, no need to calculate
            //        u.StartHour >= heuristicfilter //heuristic filter
            //        ).ToList();
            //foreach (var record in recurrenceRecords)
            //{
            //    for (int i = 1; i <= record.RecurrenceCount; i++)
            //    {
            //        DateTime? newStartHour = null;
            //        DateTime? newEndHour = null;
            //        if (record.RecurrencePeriodUnit == "Week(s)")
            //        {
            //            newStartHour = record.StartHour.Value.AddDays(7 * i);
            //            newEndHour = record.EndHour.Value.AddDays(7 * i);
            //        }
            //        if (record.RecurrencePeriodUnit == "Month(s)")
            //        {
            //            newStartHour = record.StartHour.Value.AddMonths(i);
            //            newEndHour = record.EndHour.Value.AddMonths(i);
            //        }
            //        if (newStartHour != null && newEndHour != null)
            //        {
            //            if (newStartHour <= EndTime && StartTime <= newEndHour)
            //            {
            //                return true;
            //            }
            //        }

            //    }
            //}
            //return false;
        }
        public List<MemberUnavailability> GetMemberUnavailabilitiesForRange(int MemberId, DateTime StartTime, DateTime EndTime)
        {
            return GetMemberUnavailabilitiesForRange(new List<int> { MemberId }, StartTime, EndTime);
        }
        public List<MemberUnavailability> GetMemberUnavailabilitiesForRange(IEnumerable<int> MemberIds, DateTime StartTime, DateTime EndTime)
        {
            var StartDate = StartTime.Date;
            var EndDate = EndTime.Date.AddDays(1).AddMilliseconds(-1);

            var unavailiabilitiesInExtendedRange = db.MemberUnavailabilities.Where(
                u =>
                    MemberIds.Contains(u.MemberId) &&
                    StartDate < u.EndHour &&
                    u.StartHour < EndDate
                );

            var simpleUnavailabilitiesInRange = unavailiabilitiesInExtendedRange.Where(
                u =>
                    u.IsWholeDay == false &&
                    StartTime < u.EndHour &&
                    u.EndHour < EndTime
                ).ToList();

            // Notice we should not be just matching it by day... sigh
            var wholeDayUnavailablitlitesInRange = unavailiabilitiesInExtendedRange.Where(
                    u =>
                         u.IsWholeDay == true
                ).ToList().Except(simpleUnavailabilitiesInRange);
            var heuristicfilter = StartTime.AddYears(-3);

            var recurrenceRecords = db.MemberUnavailabilities.Where(
                u =>
                    MemberIds.Contains(u.MemberId) &&
                    u.IsRecurrence == true &&
                    u.RecurrenceCount > 0 &&
                    u.StartHour < EndTime && // if the record is behind the date time, no need to calculate
                    u.EndHour > heuristicfilter //heuristic filter
            ).ToList();

            var recurrencedRecordInRange = new List<MemberUnavailability>();
            foreach (var record in recurrenceRecords)
            {
                for (int i = 1; i <= record.RecurrenceCount; i++)
                {
                    DateTime? newStartHour = null;
                    DateTime? newEndHour = null;
                    if (record.RecurrencePeriodUnit == "Day(s)")
                    {
                        newStartHour = record.StartHour.Value.AddDays(i);
                        newEndHour = record.EndHour.Value.AddDays(i);
                    }
                    if (record.RecurrencePeriodUnit == "Week(s)")
                    {
                        newStartHour = record.StartHour.Value.AddDays(7 * i);
                        newEndHour = record.EndHour.Value.AddDays(7 * i);
                    }
                    if (record.RecurrencePeriodUnit == "Month(s)")
                    {
                        newStartHour = record.StartHour.Value.AddMonths(i);
                        newEndHour = record.EndHour.Value.AddMonths(i);
                    }
                    // adjust the time to start of date and end of date if the unavailabilities is wholeday
                    if (record.IsWholeDay)
                    {
                        newStartHour = newStartHour.Value.Date;
                        newEndHour = newEndHour.Value.Date.AddDays(1); // start of next date.... haha.
                    }
                    if (newStartHour != null && newEndHour != null)
                    {
                        if (newStartHour < EndTime && StartTime < newEndHour)
                        {
                            // create new unavailabilities
                            recurrencedRecordInRange.Add(new MemberUnavailability()
                            {
                                MemberId = record.MemberId,
                                IsWholeDay = record.IsWholeDay,
                                StartHour = newStartHour,
                                EndHour = newEndHour,
                            });
                        }
                    }

                }
            }
            return simpleUnavailabilitiesInRange.Union(wholeDayUnavailablitlitesInRange).Union(recurrencedRecordInRange).ToList();
        }
    }
}