using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StoreShiftViewModel
    {
        public Store Store { get; set; }
        public Dictionary<int, List<int>> AvailableWeekNumbersForYears { get; set; }
        public Dictionary<int, Dictionary<int, DateTime>> StartDateOfWeekNbOfYr { get; set; }
        public Dictionary<int, Dictionary<int, DateTime>> EndDateOfWeekNbOfYr { get; set; }
    }

    public class StoreRangeShiftViewModel
    {
        public Store Store { get; set; }
        public Member Viewer { get; set; }
        public List<Member> Members { get; set; }
        public List<Member> BorrowedMembers { get; set; }
        public int WeekNumber { get; set; }
        public List<DateTime> WeekDays { get; set; }
        public List<WageType> WageTypes { get; set; }
        public Dictionary<Member, StaffRangeShiftViewModel> StaffShiftSummaries { get; set; }
        public Dictionary<DateTime, Sales> SalesForDay { get; set; }
        public Dictionary<DateTime, List<SalesTarget>> SalesTargetForDay { get; set; }
        public Dictionary<DateTime, WageType> WageTypeForDay { get; set; }
        public bool Reviewed { get; set; }
    }
}