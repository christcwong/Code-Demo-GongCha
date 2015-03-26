using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using GongChaWebApplication.Services;

namespace GongChaWebApplication.Controllers
{
    public class MemberProfileController : BaseController
    {
        MemberProfileService profileService;
        public MemberProfileController()
            : base()
        {
            this.profileService = new MemberProfileService(this.db);
        }
        //
        // GET: /MemberProfile/

        public ActionResult Index()
        {
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(db.Stores.Select(s => s.Location));
            }

            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                return RedirectToAction("List", new { StoreId = CurrentMember.Store.Id });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                CurrentLocation = CurrentMember.Store.Location;
                CurrentStore = CurrentMember.Store;
                return RedirectToAction("MemberDetails", new { MemberId = CurrentMember.Id });
            }
            return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
        }

        public ActionResult List(int StoreId = 0)
        {
            var store = db.Stores.Find(StoreId);
            if (store == null)
            {
                return RedirectToAction("Index");
            }
            CurrentLocation = store.Location;
            CurrentStore = store;
            if (CurrentMember == null || CurrentLocation == null)
            {
                CurrentLocation = null;
                CurrentStore = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
            }
            //IQueryable<MemberProfile> memberprofiles;
            //if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER || CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            //{
            //    memberprofiles = db.MemberProfiles.Where(p => p.Member.Store.Id == StoreId && p.Member.stateValue == (int)MemberState.ACTIVE).Include(m => m.Member);
            //}
            //else
            //{
            //    memberprofiles = db.MemberProfiles.Where(p => p.MemberId == CurrentMember.Id && p.Member.stateValue == (int)MemberState.ACTIVE).Include(m => m.Member);
            //}


            ViewBag.CurrentMember = CurrentMember;
            ViewBag.CurrentLocation = CurrentLocation;
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id == StoreId))
            {
                // it should be able to show all members of that store,
                // and members from other store that have a member profile for the selected store

                var memberOfStore = db.Members.Where(m =>
                        m.Store.Id == StoreId &&
                        m.stateValue == (int)MemberState.ACTIVE).ToList();
                var memberWithProfileOfThisStore = db.MemberProfiles
                    .Where(p =>
                        p.BasedStoreId == StoreId &&
                        p.Member.stateValue == (int)MemberState.ACTIVE)
                    .Select(p => p.Member).Include(m => m.Store).ToList();
                return View(memberOfStore.Union(memberWithProfileOfThisStore));
            }
            // Managers have no point of viewing other stores. they can click on members details instead.
            //// Mangers request view of other stores... show him only members from his store and with profiles at other stores.
            //if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && CurrentMember.Store.Id != StoreId)
            //{
            //    return View(
            //        db.MemberProfiles
            //        .Where(p =>
            //            p.BasedStoreId == StoreId &&
            //            p.Member.stateValue == (int)MemberState.ACTIVE &&
            //            p.Member.Store.Id == CurrentMember.Store.Id
            //            )
            //        .Select(p => p.Member)
            //        .Include(m => m.Store)
            //        );
            //}
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return RedirectToAction("MemberDetails", new { MemberId = CurrentMember.Id });
            }
            //return View(memberprofiles.ToList());
            return View();
        }


        public ActionResult MemberDetails(int MemberId)
        {
            if (CurrentMember == null || CurrentLocation == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            var member = db.Members.Find(MemberId);
            ViewBag.Member = member;
            ViewBag.CurrentMember = CurrentMember;
            ViewBag.CurrentLocation = CurrentLocation;
            ViewBag.CurrentStoreId = CurrentStore.Id;
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR ||
                ((CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER) && CurrentMember.Id == MemberId))
            {
                var memberProfiles = db.MemberProfiles.Where(p => p.MemberId == MemberId && p.Member.stateValue == (int)MemberState.ACTIVE).ToList();
                var memberWageViewModelList = memberProfiles.Select(p => profileService.GetMemberWageProfileViewModel(p.Id)).ToList();

                return View(memberWageViewModelList);
            }
            // Managers can only views profiles related itself.
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                var memberProfiles = db.MemberProfiles.Where(
                    p =>
                        p.MemberId == MemberId &&
                        p.Member.stateValue == (int)MemberState.ACTIVE &&
                        (p.Member.Store.Id == CurrentMember.Store.Id ||
                        p.BasedStore.Id == CurrentMember.Store.Id)
                    ).ToList();
                var memberWageViewModelList = memberProfiles.Select(p => profileService.GetMemberWageProfileViewModel(p.Id)).ToList();

                return View(memberWageViewModelList);

            }
            return View();
        }
        //
        // GET: /MemberProfile/Details/5

        public ActionResult Details(int id = 0)
        {
            if (CurrentMember == null || CurrentLocation == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            MemberProfile memberprofile = db.MemberProfiles.Find(id);
            if (memberprofile == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            var memberWageViewModel = profileService.GetMemberWageProfileViewModel(memberprofile.Id);

            ViewBag.CurrentMember = CurrentMember;
            // Need to check if this shift belongs to the user.
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(memberWageViewModel);
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && memberprofile.Member.Store.Id == CurrentMember.Store.Id)
            {
                return View(memberWageViewModel);
            }
            if ((CurrentMember.AccessLevel == MemberAccessLevels.STAFF || CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER) && memberprofile.MemberId == CurrentMember.Id)
            {
                return View(memberWageViewModel);
            }
            return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
        }

        //
        // GET: /MemberProfile/Create

        /// <summary>
        /// Create Wage Profile for Member of Id <para>Id</para>. If <para>Id</para> is omitted, can create a profile for any member
        /// </summary>
        /// <param name="id">Id of Member to create a profile.</param>
        /// <returns></returns>
        [Roles(MemberAccessLevels.STAFF, MemberAccessLevels.WAREHOUSE_MANAGER, SuccessfulRedirectMethod = RedirectMethods.INDEX, Order = 1)]
        [Roles(MemberAccessLevels.MANAGER, MemberAccessLevels.DIRECTOR, FailureRedirectMethod = RedirectMethods.SAME_REQUEST, Order = 2)]
        public ActionResult Create(int id = 0)
        {
            if (CurrentLocation == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }

            var stores = db.Stores.Where(s => s.LocationId == CurrentLocation.Id);
            if (!stores.Any())
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            CurrentStore = stores.First();

            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR || CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
            {
                Dictionary<WageType, MemberRate> dicRate = new Dictionary<WageType, MemberRate>();

                List<MemberRate> rates = new List<MemberRate>();
                foreach (var wageType in db.WageTypes)
                {
                    rates.Add(new MemberRate() { WageTypeId = wageType.Id, WageType = wageType });
                }
                MemberWageProfileViewModel vm = new MemberWageProfileViewModel
                {
                    Profile = new MemberProfile(),
                    MemberWageBasis = rates
                };
                if (id != 0)
                {
                    ViewBag.MemberId = db.Members.Where(m => m.stateValue == (int)MemberState.ACTIVE && m.Id == id).ToList()
                        .Select(
                            m => new GroupedSelectListItem
                            {
                                GroupKey = (m.Store == null ? 0 : m.Store.Id).ToString(),
                                GroupName = (m.Store == null ? "Directors" : m.Store.Name),
                                Text = m.Name,
                                Value = m.Id.ToString(),
                            }
                        );
                }
                else
                {
                    ViewBag.MemberId = db.Members.Where(m => m.stateValue == (int)MemberState.ACTIVE).ToList()
                        .Select(
                            m => new GroupedSelectListItem
                            {
                                GroupKey = (m.Store == null ? 0 : m.Store.Id).ToString(),
                                GroupName = (m.Store == null ? "Directors" : m.Store.Name),
                                Text = m.Name,
                                Value = m.Id.ToString(),
                            }
                        );
                }
                ViewBag.BasedStoreId = new SelectList(db.Stores, "Id", "Name");
                return View(vm);
            }
            return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
        }

        //
        // POST: /MemberProfile/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MemberWageProfileViewModel vm)
        {
            if (CurrentMember == null || CurrentLocation == null || CurrentMember.AccessLevel == MemberAccessLevels.STAFF)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }

            var stores = db.Stores.Where(s => s.LocationId == CurrentLocation.Id);
            if (!stores.Any())
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            CurrentStore = stores.First();

            if (ModelState.IsValid)
            {
                bool valid = true;
                var member = db.Members.Find(vm.Profile.MemberId);
                var store = db.Stores.Find(vm.Profile.BasedStoreId);

                #region Rule : Check whether the member already has a profile for that store.
                var existingProfile = db.MemberProfiles.Where(p => p.MemberId == vm.Profile.MemberId && p.BasedStoreId == vm.Profile.BasedStoreId);
                if (existingProfile.Any())
                {
                    valid = false;
                    ViewBag.StatusMessage += "There is existing profile for member " + member.Name + " at location " + store.Name + ". Please go to edit that profile instead";
                }
                #endregion

                #region Rule : Check Either Member or MemberProfile is related to current Manager
                if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER)
                {
                    if ((vm.Profile.BasedStoreId != CurrentMember.Store.Id) && (member.Store.Id != CurrentMember.Store.Id))
                    {
                        valid = false;
                        ViewBag.StatusMessage += "You are not authorized to create profile for " + member.Name + " at " + store.Name;
                    }
                }
                #endregion

                if (valid)
                {
                    vm.Profile.Rates = new List<MemberRate>();
                    foreach (var rate in vm.MemberWageBasis)
                    {
                        db.MemberRates.Add(rate);
                        vm.Profile.Rates.Add(rate);
                    }
                    db.MemberProfiles.Add(vm.Profile);
                    db.SaveChanges();
                    return RedirectToAction("List", new { StoreId = CurrentStore.Id });
                }
            }
            //ViewBag.MemberId = new SelectList(db.Members, "Id", "Email", vm.Profile.MemberId);

            foreach (var item in vm.MemberWageBasis)
            {
                item.WageType = db.WageTypes.Find(item.WageTypeId);
            }

            if ((CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR) ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER))
            {
                ViewBag.BasedStoreId = new SelectList(db.Stores, "Id", "Name", vm.Profile.BasedStoreId);
                ViewBag.MemberId = db.Members.Where(m => m.stateValue == (int)MemberState.ACTIVE).ToList()
                        .Select(
                            m => new GroupedSelectListItem
                            {
                                GroupKey = (m.Store == null ? 0 : m.Store.Id).ToString(),
                                GroupName = (m.Store == null ? "Directors" : m.Store.Name),
                                Text = m.Name,
                                Value = m.Id.ToString(),
                            }
                        );
            }
            //if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && vm.Profile.Member.Store.Id == CurrentMember.Store.Id)
            //{
            //    var memberIdWithProfileAtSameStore = profileService.GetMemberIdWithProfile(CurrentStore.Id);
            //    ViewBag.MemberId = db.Members.Where(
            //                m =>
            //                    m.MemberState == (int)MemberState.ACTIVE &&
            //                    !memberIdWithProfileAtSameStore.Contains(m.Id)
            //             ).ToList()
            //            .Select(
            //                m => new GroupedSelectListItem
            //                {
            //                    GroupKey = (m.Store == null ? 0 : m.Store.Id).ToString(),
            //                    GroupName = (m.Store == null ? "Directors" : m.Store.Name),
            //                    Text = m.Name,
            //                    Value = m.Id.ToString(),
            //                }
            //            );
            //    ViewBag.BasedStoreId = new SelectList(db.Stores.Where(s => s.Id == CurrentMember.Store.Id || s.Id == CurrentStore.Id), "Id", "Name", vm.Profile.BasedStoreId);
            //}
            return View(vm);

            /// zzzzzzzz
        }

        //
        // GET: /MemberProfile/Edit/5

        public ActionResult Edit(int id = 0)
        {
            if (CurrentMember == null || CurrentLocation == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            MemberProfile memberprofile = db.MemberProfiles.Find(id);

            if (memberprofile == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            //MemberWageProfileViewModel vm = GetMemberWageProfileViewModel(memberprofile.MemberId);
            MemberWageProfileViewModel vm = profileService.GetMemberWageProfileViewModel(id);
            //ViewBag.MemberId = new SelectList(db.Members, "Id", "Email", memberprofile.MemberId);

            // Find Available base store.
            var StoreIdOfProfilesOfMember = db.MemberProfiles.Where(
                p =>
                    p.MemberId == memberprofile.MemberId &&
                    p.Id != memberprofile.Id
                ).Select(p => p.BasedStoreId).ToList();

            ViewBag.BasedStoreId = new SelectList(
                db.Stores.Where(
                    s =>
                        !StoreIdOfProfilesOfMember.Contains(s.Id)
                )
                , "Id", "Name", memberprofile.BasedStoreId);


            //return View(vm);
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(vm);
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER &&
                (memberprofile.Member.Store.Id == CurrentMember.Store.Id ||
                memberprofile.BasedStoreId == CurrentMember.Store.Id))
            {
                return View(vm);
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF && memberprofile.MemberId == CurrentMember.Id)
            {
                return RedirectToAction("Detail", new { id = id });
            }
            return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
        }

        //
        // POST: /MemberProfile/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MemberWageProfileViewModel vm)
        {
            if (CurrentMember == null || CurrentLocation == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
            }
            var stores = db.Stores.Where(s => s.LocationId == CurrentLocation.Id);
            var member = db.Members.Find(vm.Profile.MemberId);
            if (!stores.Any() || member == null)
            {
                return RedirectToAction("List", new { StoreId = vm.Profile.BasedStoreId });
            }
            CurrentStore = stores.First();
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR ||
                (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER &&
                (member.Store.Id == CurrentMember.Store.Id || vm.Profile.BasedStoreId == CurrentMember.Store.Id)))
            {
                if (ModelState.IsValid)
                {
                    db.Entry(vm.Profile).State = EntityState.Modified;
                    foreach (var item in vm.MemberWageBasis)
                    {
                        db.Entry(item).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    return RedirectToAction("List", new { StoreId = CurrentStore.Id });
                }
            }
            else
            {
                ViewBag.StatusMessage = "You are not authorized to edit this profile";
            }
            //ViewBag.MemberId = new SelectList(db.Members, "Id", "Email", vm.Profile.MemberId);
            //ViewBag.BasedStoreId = new SelectList(db.Stores, "Id", "Name", vm.Profile.BasedStoreId);
            // Find Available base store.
            var StoreIdOfProfilesOfMember = db.MemberProfiles.Where(
                p =>
                    p.MemberId == vm.Profile.MemberId &&
                    p.Id != vm.Profile.Id
                ).Select(p => p.BasedStoreId).ToList();

            ViewBag.BasedStoreId = new SelectList(
                db.Stores.Where(
                    s =>
                        !StoreIdOfProfilesOfMember.Contains(s.Id)
                )
                , "Id", "Name", vm.Profile.BasedStoreId);
            vm.WageTypes = db.WageTypes.ToList();
            foreach (var wageBase in vm.MemberWageBasis)
            {
                wageBase.WageType = db.WageTypes.Find(wageBase.WageTypeId);
            }
            return View(vm);
        }

        //
        // GET: /MemberProfile/Delete/5

        public ActionResult Delete(int id = 0)
        {
            if (CurrentMember == null || CurrentLocation == null)
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
            }
            MemberProfile memberprofile = db.MemberProfiles.Find(id);
            if (memberprofile == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Url.Action("Index") });
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return View(memberprofile);
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER && memberprofile.Member.Store.Id == CurrentMember.Store.Id)
            {
                return View(memberprofile);
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.STAFF && memberprofile.MemberId == CurrentMember.Id)
            {
                return View(memberprofile);
            }
            return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
        }

        //
        // POST: /MemberProfile/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (CurrentLocation == null || !Accessible())
            {
                CurrentLocation = null;
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
            }
            MemberProfile memberprofile = db.MemberProfiles.Find(id);
            db.MemberProfiles.Remove(memberprofile);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}