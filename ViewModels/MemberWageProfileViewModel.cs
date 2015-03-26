using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class MemberWageProfileViewModel
    {
        //public Member Member { get; set; }
        public MemberProfile Profile { get; set; }

        // this is not going to be a good postback.
        //public Dictionary<WageType, MemberRate> WageBase { get; set; }
        // So we actually use the following for postback:
        public List<WageType> WageTypes { get; set; }
        public List<MemberRate> MemberWageBasis { get; set; }

    }
}