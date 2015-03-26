using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using log4net;
using System.Reflection;
using System.Diagnostics;

namespace GongChaWebApplication.Controllers
{
    [Logging]
    [Roles("ANY", FailureRedirectMethod = RedirectMethods.SAME_REQUEST)]
    public class BaseController : Controller
    {
        protected GongChaDbContext db = new GongChaDbContext();

        protected readonly ILog log = LogManager.GetLogger('\t'.ToString());

        protected void Log(String message = "")
        {
            MethodBase callingMethod = new StackFrame(1).GetMethod();
            this.log.Info((callingMethod == null ? "null" : "." + callingMethod.Name) + " Current Member : " + (CurrentMember == null ? "null" : CurrentMember.Email) + " called." + (message == null ? "" : " Details: " + message));
        }



        protected Member CurrentMember
        {
            get
            {
                if (Session["CurrentMember"] == null)
                {
                    return null;
                }

                return Session["CurrentMember"] as Member;
            }
            set { Session["CurrentMember"] = value; }
        }

        protected Store CurrentStore
        {
            get
            {
                if (Session["CurrentStore"] == null)
                {
                    return null;
                }

                return Session["CurrentStore"] as Store;
            }
            set { Session["CurrentStore"] = value; }
        }

        protected Location CurrentLocation
        {
            get
            {
                if (Session["CurrentLocation"] == null)
                {
                    return null;
                }

                return Session["CurrentLocation"] as Location;
            }
            set { Session["CurrentLocation"] = value; }
        }

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    base.OnActionExecuting(filterContext);


        //    var actionName = filterContext.ActionDescriptor.ActionName;

        //    if (CurrentMember == null)
        //    {
        //        filterContext.Result = RedirectToAction("Logout", new { returnUrl = Request.Url.PathAndQuery });
        //    }
        //}

        [NonAction]
        public Boolean Accessible()
        {
            if (CurrentMember == null)
            {
                return false;
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return true;
            }
            if (CurrentLocation != null)
            {
                //return True for all warehouse
                if (AccessibleLocations().Contains(CurrentLocation.Id))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [NonAction]
        public IEnumerable<int> AccessibleLocations()
        {
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
                return db.Locations.Select(loc => loc.Id).ToList();
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.WAREHOUSE_MANAGER)
            {
                return db.Warehouses.Select(wh => wh.LocationId).ToList().Union(new List<int> { CurrentMember.Store.LocationId }).Distinct();
            }
            if (CurrentMember.AccessLevel == MemberAccessLevels.MANAGER || CurrentMember.AccessLevel == MemberAccessLevels.STAFF)
            {
                return new List<int> { CurrentMember.Store.LocationId };
            }
            return new List<int> { };
        }
    }
}
