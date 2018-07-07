using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc4_Kulti.Filters;

namespace Mvc4_Kulti.Controllers
{
  [Authorize]
  [InitializeSimpleMembership]

    public class AgencyController : Controller
    {
        private AgenciesEntities db = new AgenciesEntities();

        //
        // GET: /Agency/

        public ActionResult Index()
        {
            return View(db.Agencies.ToList());
        }

        //
        // GET: /Agency/Details/5

        public ActionResult Details(int id = 0)
        {
            Agencies agencies = db.Agencies.Single(a => a.AgencyId == id);
            if (agencies == null)
            {
                return HttpNotFound();
            }
            return View(agencies);
        }

        //
        // GET: /Agency/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Agency/Create

        [HttpPost]
        public ActionResult Create(Agencies agencies)
        {
            if (ModelState.IsValid)
            {
                db.Agencies.AddObject(agencies);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(agencies);
        }

        //
        // GET: /Agency/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Agencies agencies = db.Agencies.Single(a => a.AgencyId == id);
            if (agencies == null)
            {
                return HttpNotFound();
            }
            return View(agencies);
        }

        //
        // POST: /Agency/Edit/5

        [HttpPost]
        public ActionResult Edit(Agencies agencies)
        {
            if (ModelState.IsValid)
            {
                db.Agencies.Attach(agencies);
                db.ObjectStateManager.ChangeObjectState(agencies, System.Data.EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agencies);
        }

        //
        // GET: /Agency/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Agencies agencies = db.Agencies.Single(a => a.AgencyId == id);
            if (agencies == null)
            {
                return HttpNotFound();
            }
            return View(agencies);
        }

        //
        // POST: /Agency/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Agencies agencies = db.Agencies.Single(a => a.AgencyId == id);
            db.Agencies.DeleteObject(agencies);
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