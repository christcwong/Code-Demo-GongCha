using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class MemberUnavailability
    {
        [Key]
        public int Id { get; set; }

        public int MemberId { get; set; }
        public virtual Member Memeber { get; set; }

        [DisplayName("Whole Day")]
        public bool IsWholeDay { get; set; }

        // If Is Whole Day equal to false, the following two fields should be referenced.
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}", ApplyFormatInEditMode = true)]
        [DisplayName("Start of Working Hour")]
        public DateTime? StartHour { get; set; }

        [DisplayName("End of Working Hour")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? EndHour { get; set; }

        [DisplayName("Recurrence")]
        public bool IsRecurrence { get; set; }
        // If the unavailablity is recurrence, 
        // It should be able to calculate when next slot that the staff will be unavailable 

        // e.g. for a recurrence of period of one week
        [DisplayName("Recurrence Period")]
        public Double? RecurrencePeriod { get; set; }
        [DisplayName("Recurrence Unit")]
        public string RecurrencePeriodUnit { get; set; }

        // Number of Recurrence Until there is no more occurence of unavailablility
        // e.g. for an unavailability of two weeks, it should be set to "1" to indicate it happened again for 1 time
        [Range(0, Int32.MaxValue, ErrorMessage = "Value should be non-negative")]
        [DisplayName("Recurrence Count")]
        public int? RecurrenceCount { get; set; }
    }
}