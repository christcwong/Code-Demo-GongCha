using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace GongChaWebApplication.Controllers
{
    public class RosterController : BaseController
    {
        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
            }
            //return View();
            return RedirectToAction("Index", "StoreShiftSummary");
        }
    }
}
