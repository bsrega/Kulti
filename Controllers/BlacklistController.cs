using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc4_Kulti.Filters;
using Mvc4_Kulti.ViewModels;
using WebMatrix.WebData;

namespace Mvc4_Kulti.Controllers
{

  [Authorize]
  [InitializeSimpleMembership]
  
  public class BlacklistController : Controller
  {

    private readonly BlacklistEntities db = new BlacklistEntities();

    //
    // GET: /Blacklist/

    public ActionResult Index()
    {      
      var hlpView = new HelpersViewModel();
      return View(hlpView.GetBlacklist());
    }

    //
    // GET: /Blacklist/List

    public ActionResult List()
    {
      return View(db.Blacklist.ToList());
    }

    //
    // GET: ValidList
    public ActionResult ValidList()
    {
      var blackList = new List<Blacklist> {};
      List<Blacklist> u = db.Blacklist.ToList()
        .Where(e => e.FromDate < DateTime.Now && e.ToDate > DateTime.Now)
        .OrderBy(s => s.LastName).ToList();
      foreach (Blacklist blk in u)
      {
        var b = new Blacklist();
        b.BlacklistID = blk.BlacklistID;
        b.FirstName = blk.FirstName;
        b.LastName = blk.LastName;
        b.Reason = blk.Reason;
        blackList.Add(b);
      }
      return View(blackList);
    }


    //
    // GET: /Blacklist/Details/5

    public ActionResult Details(int id = 0)
    {
      Blacklist blacklist = db.Blacklist.Single(b => b.BlacklistID == id);
      if (blacklist == null)
      {
        return HttpNotFound();
      }
      return View(blacklist);
    }

    //
    // GET: /Blacklist/Create

    public ActionResult Create()
    {
      return View();
    }

    //
    // POST: /Blacklist/Create

    [HttpPost]
    public ActionResult Create(Blacklist blacklist,  HttpPostedFileBase img)
    {
      string targetPath = string.Empty;
      HelpersViewModel HlpView = new HelpersViewModel();
      if (ModelState.IsValid)
      {
        blacklist.UserID = WebSecurity.GetUserId(User.Identity.Name);
        db.Blacklist.AddObject(blacklist);
        db.SaveChanges();

        //save img
        if (img != null)
        {
          targetPath = Server.MapPath("~/Images/Blacklist/" + blacklist.BlacklistID + ".jpg");
          Image sourceImg = Image.FromStream(img.InputStream);
          Image resized = HlpView.ResizeImage(sourceImg, new Size(320, 415));
          resized.Save(targetPath, ImageFormat.Jpeg);
        }
        
        return RedirectToAction("List");
      }

      TempData["Success"] = "Der Eintrag wurde erstellt";
      return View(blacklist);
    }

    //
    // GET: /Blacklist/Edit/5

    public ActionResult Edit(int id = 0)
    {
      Blacklist blacklist = db.Blacklist.Single(b => b.BlacklistID == id);
      if (blacklist == null)
      {
        return HttpNotFound();
      }
      return View(blacklist);
    }

    //
    // POST: /Blacklist/Edit/5

    [HttpPost]
    public ActionResult Edit(Blacklist blacklist)
    {
      if (ModelState.IsValid)
      {
        blacklist.UserID = WebSecurity.GetUserId(User.Identity.Name);
        db.Blacklist.Attach(blacklist);
        db.ObjectStateManager.ChangeObjectState(blacklist, EntityState.Modified);
        db.SaveChanges();
        TempData["Success"] = "Der Eintrag wurde aktualisiert";
        return RedirectToAction("List");
      }
      return View(blacklist);
    }

    //
    // GET: /Blacklist/Delete/5

    public ActionResult Delete(int id = 0)
    {
      Blacklist blacklist = db.Blacklist.Single(b => b.BlacklistID == id);
      if (blacklist == null)
      {
        return HttpNotFound();
      }
      return View(blacklist);
    }

    //
    // POST: /Blacklist/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      try
      {
        Blacklist blacklist = db.Blacklist.Single(b => b.BlacklistID == id);
        db.Blacklist.DeleteObject(blacklist);
        db.SaveChanges();
        TempData["Success"] = "Der Eintrag wurde gelöscht";
        return RedirectToAction("List");
      }
      catch (Exception ex)
      {
        TempData["Success"] = "Fehler: " + ex.Message;
        throw;
      }


    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}