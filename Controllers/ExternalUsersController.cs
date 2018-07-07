using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc4_Kulti.Controllers
{
    public class ExternalUsersController : Controller
    {
        private ExternalUsersEntities db = new ExternalUsersEntities();

        //
        // GET: /ExternalUsers/

        public ActionResult Index()
        {
            return View(db.ExternalUsers.ToList());
        }

        //
        // GET: /ExternalUsers/Details/5

        public ActionResult Details(int id = 0)
        {
            ExternalUsers externalusers = db.ExternalUsers.Single(e => e.ID == id);
            if (externalusers == null)
            {
                return HttpNotFound();
            }
            return View(externalusers);
        }

        //
        // GET: /ExternalUsers/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ExternalUsers/Create

        [HttpPost]
        public ActionResult Create(ExternalUsers externalusers)
        {
            if (ModelState.IsValid)
            {
                db.ExternalUsers.AddObject(externalusers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(externalusers);
        }

        //
        // GET: /ExternalUsers/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ExternalUsers externalusers = db.ExternalUsers.Single(e => e.ID == id);
            if (externalusers == null)
            {
                return HttpNotFound();
            }
            return View(externalusers);
        }

        //
        // POST: /ExternalUsers/Edit/5

        [HttpPost]
        public ActionResult Edit(ExternalUsers externalusers)
        {
            if (ModelState.IsValid)
            {
                db.ExternalUsers.Attach(externalusers);
                db.ObjectStateManager.ChangeObjectState(externalusers, System.Data.EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(externalusers);
        }

        //
        // GET: /ExternalUsers/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ExternalUsers externalusers = db.ExternalUsers.Single(e => e.ID == id);
            if (externalusers == null)
            {
                return HttpNotFound();
            }
            return View(externalusers);
        }

        //
        // POST: /ExternalUsers/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ExternalUsers externalusers = db.ExternalUsers.Single(e => e.ID == id);
            db.ExternalUsers.DeleteObject(externalusers);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}