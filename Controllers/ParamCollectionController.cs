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

    public class ParamCollectionController : Controller
    {
        private ParamColEntities db = new ParamColEntities();

        //
        // GET: /ParamCollection/

        public ActionResult Index()
        {
            return View(db.ParamCollection.OrderBy(e => e.Type).ToList());
        }

        //
        // GET: /ParamCollection/Details/5

        public ActionResult Details(int id = 0)
        {
            ParamCollection paramcollection = db.ParamCollection.Single(p => p.id == id);
            if (paramcollection == null)
            {
                return HttpNotFound();
            }
            return View(paramcollection);
        }

        //
        // GET: /ParamCollection/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ParamCollection/Create

        [HttpPost]
        public ActionResult Create(ParamCollection paramcollection)
        {
            if (ModelState.IsValid)
            {
                db.ParamCollection.AddObject(paramcollection);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(paramcollection);
        }

        //
        // GET: /ParamCollection/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ParamCollection paramcollection = db.ParamCollection.Single(p => p.id == id);
            if (paramcollection == null)
            {
                return HttpNotFound();
            }
            return View(paramcollection);
        }

        //
        // POST: /ParamCollection/Edit/5

        [HttpPost]
        public ActionResult Edit(ParamCollection paramcollection)
        {
            if (ModelState.IsValid)
            {
                db.ParamCollection.Attach(paramcollection);
                db.ObjectStateManager.ChangeObjectState(paramcollection, System.Data.EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paramcollection);
        }

        //
        // GET: /ParamCollection/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ParamCollection paramcollection = db.ParamCollection.Single(p => p.id == id);
            if (paramcollection == null)
            {
                return HttpNotFound();
            }
            return View(paramcollection);
        }

        //
        // POST: /ParamCollection/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ParamCollection paramcollection = db.ParamCollection.Single(p => p.id == id);
            db.ParamCollection.DeleteObject(paramcollection);
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