using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GongChaWebApplication.Models
{
    public class ShiftPeriodViewModel
    {
        //public int MemberId { get; set; }
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }
        public int StoreId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DateTime> Days { get; set; }
        public virtual Store Store { get; set; }
        public List<Member> Members { get; set; }
        public List<Member> BorrowedMembers { get; set; }
        public List<WageType> WageTypes { get; set; }
        //public List<SelectListItem> BreakTime { get; set; }
        public Dictionary<Member, ShiftPeriodMemberViewModel> MemberDetails { get; set; }

        // For UI display
        public int? NumberOfWeeksToView { get; set; }
    }

    // it is created to avoid a dictionary of dictionary... brain damaging.
    public class ShiftPeriodMemberViewModel
    {
        public int MemberId { get; set; }
        public virtual Member Member { get; set; }
        public Dictionary<DateTime, ShiftPeriodMemberDayViewModel> DayDetails { get; set; }
        //public List<Shift> Shifts { get; set; }
        //public List<MemberUnavailability> MemberUnavailabilities { get; set; }
    }

    public class ShiftPeriodMemberDayViewModel
    {
        public int MemberId { get; set; }
        public virtual Member Member { get; set; }
        public DateTime Date { get; set; }
        public List<ShiftEditViewModel> Shifts { get; set; }
        public List<MemberUnavailability> MemberUnavailabilities { get; set; }
    }

    public class ShiftEditViewModel
    {
        public int? rowId { get; set; }
        public DateTime? Date { get; set; }
        public Shift Shift { get; set; }
        public Boolean MarkDeleted { get; set; }
    }

    public class ShiftBatchEditViewModel
    {
        public List<ShiftEditViewModel> EditModels { get; set; }
    }

}