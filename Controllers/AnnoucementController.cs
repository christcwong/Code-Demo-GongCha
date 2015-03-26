using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;
using GongChaWebApplication.Helpers;

namespace GongChaWebApplication.Controllers
{
    public class AnnoucementController : BaseController
    {

        //
        // GET: /Annoucement/

        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { ReturnUrl = Request.Url.PathAndQuery });
            }

            var messages = db.Messages.Where(m => m.IsAnnoucement == true).Include(m => m.Writer);
            if (CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR)
            {
            }
            else
            {
                messages = messages.Where(
                    m =>
                        m.Writer.AccessLevel == MemberAccessLevels.DIRECTOR ||
                        m.Writer.Store.Id == CurrentMember.Store.Id
                );
            }
            return View(messages.OrderByDescending(
                    m =>
                        (m.EditTime.HasValue ?
                            (m.EditTime.Value < m.PostTime ? m.PostTime : m.EditTime.Value) :
                            m.PostTime
                        )
                ).ToList());
        }

        //
        // GET: /Annoucement/Details/5

        public ActionResult Details(int id = 0)
        {
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        //
        // GET: /Annoucement/Create

        public ActionResult Create()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Index");
            }
            var msg = new Message()
            {
                Writer = CurrentMember,
                WriterId = CurrentMember.Id,
                IsAnnoucement = true
            };
            return View(msg);
        }

        //
        // POST: /Annoucement/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Message message)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                message.PostTime = GongChaDateTimeWrapper.Now;
                message.EditTime = null;
                db.Messages.Add(message);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(message);
        }

        //
        // GET: /Annoucement/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            ViewBag.WriterId = new SelectList(db.Members, "Id", "Email", message.WriterId);
            return View(message);
        }

        //
        // POST: /Annoucement/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Message message)
        {
            if (ModelState.IsValid)
            {
                db.Entry(message).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.WriterId = new SelectList(db.Members, "Id", "Email", message.WriterId);
            return View(message);
        }

        //
        // GET: /Annoucement/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        //
        // POST: /Annoucement/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Message message = db.Messages.Find(id);
            db.Messages.Remove(message);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}
    }
}