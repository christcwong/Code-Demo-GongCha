using GongChaWebApplication.Controllers;
using GongChaWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Services
{
    public class MemberProfileService : BaseController
    {
        public MemberProfileService(GongChaDbContext db)
        {
            this.db = db;
        }

        public List<int> GetMemberIdWithProfile(int StoreId)
        {
            return db.MemberProfiles
                .Where(p => p.BasedStoreId == StoreId
                    && p.Member.stateValue == (int)MemberState.ACTIVE)
                    .Select(p => p.MemberId)
                    .Distinct()
                    .ToList();
        }

        #region Get Member Wage Profile View Model

        public MemberWageProfileViewModel GetMemberWageProfileViewModel(int profileId)
        {
            var Profile = db.MemberProfiles.Find(profileId);
            if (Profile == null)
            {
                return null;
            }
            return GetMemberWageProfileViewModel(Profile);
        }
        public MemberWageProfileViewModel GetMemberWageProfileViewModel(int memberId, int storeId)
        {
            Member member = db.Members.Find(memberId);
            Store store = db.Stores.Find(storeId);
            if (member == null || store == null)
            {
                return null;
            }
            return GetMemberWageProfileViewModel(member, store);
        }

        public MemberWageProfileViewModel GetMemberWageProfileViewModel(Member member, Store store)
        {
            MemberProfile profile = db.MemberProfiles.Where(p => p.MemberId == member.Id && p.BasedStoreId == store.Id).FirstOrDefault();
            if (profile == null)
            {
                return null;
            }
            return GetMemberWageProfileViewModel(profile);
        }
        public MemberWageProfileViewModel GetMemberWageProfileViewModel(MemberProfile profile)
        {
            MemberWageProfileViewModel vm = new MemberWageProfileViewModel()
            {
                Profile = profile,
                MemberWageBasis = db.MemberRates.Where(r => r.MemberProfileId == profile.Id).ToList()
            };
            return vm;
        }
        #endregion
    }
}