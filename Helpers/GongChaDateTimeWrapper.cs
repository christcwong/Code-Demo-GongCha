using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace GongChaWebApplication.Helpers
{
    public static class GongChaDateTimeWrapper
    {
        private static TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. Australia Standard Time");
        public static DateTime Now { get { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone); } }

        public static DateTime StartDateOfYear(int year)
        {
            return new DateTime(year, 7, 1);
        }
        public static DateTime FirstMondayOfYear(int year)
        {
            var startDate = StartDateOfYear(year).Date;
            return startDate.AddDays(-(startDate.DayOfWeek - DayOfWeek.Monday)).Date;
        }
        public static int Year(DateTime datetime)
        {
            return (datetime.Month < 7 ? datetime.Year - 1 : datetime.Year);
        }

        public static DateTime StartOfWeek(DateTime date)
        {
            return date.AddDays(-(date.DayOfWeek - DayOfWeek.Monday)).Date;
        }

        public static DateTime StartOfWeek(int year, int weekNb)
        {
            if (weekNb >= 1 && weekNb <= 52)
            {
                var firstMonday = FirstMondayOfYear(year);
                return firstMonday.AddDays(7 * (weekNb - 1));
            }
            throw new ArgumentOutOfRangeException("weekNb", weekNb, "Week Number can only be within 1 to 52");
        }

        public static DateTime EndOfWeek(DateTime date)
        {
            return StartOfWeek(date).AddDays(6).Date.AddDays(1).AddMilliseconds(-1);
        }

        public static DateTime EndOfWeek(int year, int weekNb)
        {
            return StartOfWeek(year, weekNb).AddDays(6).Date;
        }

        public static int WeekNumber(DateTime Date)
        {
            var date = Date.Date;
            DateTime firstMonday = FirstMondayOfYear(date.Year);
            if (date < firstMonday)
            {
                firstMonday = FirstMondayOfYear(date.Year - 1);
            }
            TimeSpan timespan = (date - firstMonday);
            return (int)(timespan.TotalDays / 7) + 1;
        }

        public static string WeekNumberWithDateLabel(int year, int weekNb)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(weekNb);
            sb.Append(" : ");
            sb.Append(StartOfWeek(year, weekNb));
            sb.Append(" - ");
            sb.Append(EndOfWeek(year, weekNb));
            return sb.ToString();
        }
    }
}