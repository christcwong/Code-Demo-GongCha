using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class StaffShiftViewModel
    {
        public Member Member { get; set; }
        public Store Store { get; set; }
        public Dictionary<int, List<int>> AvailableWeekNumbersForYears { get; set; }
    }

    public class StaffRangeShiftViewModel
    {
        public Member Member { get; set; }
        public List<MemberProfile> MemberProfiles { get; set; }
        public int WeekNumber { get; set; }
        public List<DateTime> WeekDays { get; set; }
        public List<Shift> Shifts { get; set; }
        public List<MemberRate> MemberRates { get; set; }
        public Dictionary<MemberProfile, List<MemberRate>> RatesForProfile { get; set; }
        public List<WageType> WageTypes { get; set; }
    }

}