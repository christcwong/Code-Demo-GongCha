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
    public class MessageController : BaseController
    {
        //
        // GET: /Message/

        public ActionResult Index()
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { returnUrl = Request.Url.PathAndQuery });
            }
            var messages = db.Messages.OrderByDescending(
                    m =>
                        (m.EditTime.HasValue ?
                            (m.EditTime.Value < m.PostTime ? m.PostTime : m.EditTime.Value) :
                            m.PostTime
                        )
                ).Include(m => m.Writer);
            ViewBag.CurrentMember = CurrentMember;
            return View(messages.ToList());
        }

        //
        // GET: /Message/Details/5

        public ActionResult Details(int id = 0)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { returnUrl = Request.Url.PathAndQuery });
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentMember = CurrentMember;
            return View(message);
        }

        //
        // GET: /Message/ViewThread/5

        /// <summary>
        /// View Message Thread with head message of id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ViewThread(int id)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { returnUrl = Request.Url.PathAndQuery });
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            // exhausting the thread.
            var messageProcessList = new List<Message>() { message };
            var messageResultList = new List<Message>();
            while (messageProcessList.Any())
            {
                var messageAddProcessList = new List<Message>();
                var messageRemoveProcessList = new List<Message>();
                foreach (var msg in messageProcessList)
                {
                    messageRemoveProcessList.Add(msg);
                    messageResultList.Add(msg);
                    if (msg.ChildMessages.Any())
                    {
                        messageAddProcessList.AddRange(msg.ChildMessages);
                    };
                }
                foreach (var msg in messageRemoveProcessList)
                {
                    messageProcessList.Remove(msg);
                }
                messageProcessList.AddRange(messageAddProcessList);
            }
            ViewBag.CurrentMember = CurrentMember;
            return View(messageResultList);
        }

        public ActionResult Reply(int id = 0)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { returnUrl = Request.Url.PathAndQuery });
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            MessageReplyViewModel vm = new MessageReplyViewModel()
            {
                ParentMessage = message,
                NewMessage = new Message()
                {
                    Writer = CurrentMember,
                    WriterId = CurrentMember.Id,
                    PostTime = GongChaDateTimeWrapper.Now,
                    Title = "Reply : " + message.Title
                }
            };
            ViewBag.CurrentMember = CurrentMember;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(MessageReplyViewModel vm)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Logout", "Member", new { returnUrl = Request.Url.PathAndQuery });
            }
            if (ModelState.IsValid)
            {
                var parentMessage = db.Messages.Find(vm.ParentMessage.Id);
                parentMessage.ChildMessages.Add(vm.NewMessage);
                vm.NewMessage.PostTime = GongChaDateTimeWrapper.Now;
                db.Entry(parentMessage).State = EntityState.Modified;
                db.Messages.Add(vm.NewMessage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.WriterId = new SelectList(db.Members, "Id", "Email", message.WriterId);
            ViewBag.CurrentMember = CurrentMember;
            return View(vm);
        }

        //
        // GET: /Message/Create

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
                PostTime = GongChaDateTimeWrapper.Now
            };
            ViewBag.CurrentMember = CurrentMember;
            return View(msg);
        }

        //
        // POST: /Message/Create

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
                db.Messages.Add(message);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CurrentMember = CurrentMember;
            return View(message);
        }

        //
        // GET: /Message/Edit/5

        public ActionResult Edit(int id = 0)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Index");
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            if ((CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR) ||
                (CurrentMember.Id == message.WriterId)
                )
            {
                ViewBag.CurrentMember = CurrentMember;
                return View(message);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //
        // POST: /Message/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Message message)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Index");
            }
            if ((CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR) ||
                (CurrentMember.Id == message.WriterId)
                )
            {
                if (ModelState.IsValid)
                {
                    message.EditTime = GongChaDateTimeWrapper.Now;
                    db.Entry(message).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.StatusMessage += "You are not authorized to edit this message.";
            }
            ViewBag.CurrentMember = CurrentMember;
            return View(message);
        }

        //
        // GET: /Message/Delete/5

        public ActionResult Delete(int id = 0)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Index");
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            if ((CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR) ||
                (CurrentMember.Id == message.WriterId)
                )
            {
                ViewBag.CurrentMember = CurrentMember;
                return View(message);
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /Message/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (CurrentMember == null)
            {
                return RedirectToAction("Index");
            }
            Message message = db.Messages.Find(id);
            if ((CurrentMember.AccessLevel == MemberAccessLevels.DIRECTOR) ||
                (CurrentMember.Id == message.WriterId)
                )
            {
                db.Messages.Remove(message);
                db.SaveChanges();
            }
            else
            {
                ViewBag.StatusMessage += "You are not authorized to delete this message.";
            }
            return RedirectToAction("Index");
        }


    }
}