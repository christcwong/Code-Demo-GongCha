using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;

namespace GongChaWebApplication.Controllers
{
    [Authorize]
    public class InventoryController : BaseController
    {
        //private GongChaDbContext db = new GongChaDbContext();

        //
        // GET: /Default1/

        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Path });
            }

            return RedirectToAction("Index", "StockCheck");
        }
    }
}