using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class MemberRate
    {
        [Key]
        public int Id { get; set; }

        public int MemberProfileId { get; set; }
        public virtual MemberProfile MemberProfile { get; set; }

        public int WageTypeId { get; set; }
        [DisplayName("Wage Type")]
        public virtual WageType WageType { get; set; }
        public double WageBase { get; set; }
    }
}