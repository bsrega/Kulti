using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Mvc4_Kulti.Filters;
using Mvc4_Kulti.ViewModels;
using WebMatrix.WebData;

namespace Mvc4_Kulti.Controllers
{
  [Authorize]
  [InitializeSimpleMembership]
  public class KultiEventsController : Controller
  {
    private readonly HelpersViewModel HlpView = new HelpersViewModel();
    private readonly KultiEventsModel db = new KultiEventsModel();

    //
    // GET: /KultiEvents/
    public ActionResult Index(bool oldView = false)
    {
      if (oldView)
      { //get past events
        return View(db.Events.ToList().Where(e => e.EvStartDate <= DateTime.Now).OrderByDescending(e => e.EvStartDate).ToList());
      }
      return View(db.Events.ToList().Where(e => e.EvStartDate >= DateTime.Now).OrderByDescending(e => e.EvStartDate).ToList());
    }

    //
    // GET: /KultiEvents/Calendar
    public ViewResult Calendar()
    {
      return View();
    }

    //
    // GET: Returns view for minical (not in use)
    [AllowAnonymous]
    public PartialViewResult EventsMiniCal()
    {
      return PartialView();
    }

    //public ViewResult EventsMeetingsCal()
    [AllowAnonymous]
    public PartialViewResult EventsMeetingsCal()
    {
      var hlpView = new HelpersViewModel();
      var m = new List<Tuple<Events, string>>();
      List<Events> u = db.Events.Where(
              e => e.EvEndDate >= DateTime.Now && (e.EvType == null || e.EvType != 20) && e.EvStatus == 2)
              .OrderBy(e => e.EvStartDate).ToList();

      if (u.Count == 0)
      {
        return PartialView();
      }

      foreach (Events meeting in u)
      {
        string sLocations = string.Empty;
        IEnumerable<Tuple<string, int, bool>> location =
          hlpView.GetLocations(meeting.EventId).Where(e => e.Item3.ToString() == "True");
        foreach (var s in location)
        {
          sLocations += s.Item1;
          if (location.Count().ToString() != "1")
          {
            sLocations += ", ";
          }
        }
        m.Add(new Tuple<Events, string>(meeting, sLocations));
      }
      return PartialView(m);
    }

    //public ViewResult Calendar()
    [AllowAnonymous]
    public JsonResult CalendarJson(double start, double end)
    {
      List<Events> repository = db.Events.Where(e => e.EvType == null || e.EvType != 20).ToList();
      var clientList = new List<object>();
      foreach (Events e in repository)
      {
        try
        {
          clientList.Add(new
          {
            id = e.EventId,
            title = e.EvTitle ?? "no title",
            description = e.EvText ?? "no descr",
            start = ToUnixTimestamp(e.EvStartDate.Value),
            end = ToUnixTimestamp(e.EvEndDate.Value),
            allDay = false,
            url = e.EventId
          });
        }
        catch (Exception ex)
        {
          Console.Write(ex.Message);
        }
        
      }
      return Json(clientList.ToArray(), "text/html; charset=utf-8", JsonRequestBehavior.AllowGet);
    }

    //
    // GET: /KultiEvents/ListEvents/
    [AllowAnonymous]
    public ActionResult ListEvents()
    {
      var hlpView = new HelpersViewModel();
      var eventList = new List<ExtEvent> {};

      List<Events> u = db.Events.ToList().Where(
        e => (e.EvEndDate >= DateTime.Now) && e.EvType == 20 && e.EvStatus == 2)
                   .OrderBy(e => e.EvStartDate).ToList();

      foreach (Events events in u)
      {
        var Event = new ExtEvent();
        Event.EvTitle = events.EvTitle;
        Event.EvText = events.EvText;
        Event.EvAdmission = events.EvAdmission;
        Event.EvImgPath1 = events.EvImgPath1;
        Event.EventLocations = hlpView.GetLocations(events.EventId);
        Event.EvStartDate = events.EvStartDate;
        Event.EvEndDate = events.EvEndDate;
        eventList.Add(Event);
      }
      return PartialView(eventList);
    }


    // GET: /KultiEvents/EventsPrint
    public ActionResult EventsPrint()
    {
      return View();
    }

    //
    // Post: /KultiEvents/EventsPrint
    [HttpPost]
    public ActionResult EventsPrint(string StartDate, string EndDate)
    {
      DateTime startDate = Convert.ToDateTime(StartDate);
      DateTime endDate = Convert.ToDateTime(EndDate);
      var lstEvents = new List<Tuple<Events, string, string, string>>();
      var hlp = new HelpersViewModel();
      string sSize = string.Empty;
      string sGroup = string.Empty;

      List<Events> u = db.Events.Where(
                    e => e.EvStartDate >= startDate && e.EvEndDate <= endDate && e.EvType == 20)
                  .OrderBy(e => e.EvStartDate).ToList();

      foreach (Events e in u)
      {
        sSize = hlp.GetEventSize(e.EvSize ?? 0);
        sGroup = hlp.GetEventGroup(e.EvGroupId ?? 0);
        lstEvents.Add(new Tuple<Events, string, string, string>
                        (e, sSize, sGroup, e.EvResponsible));
      }

      ViewBag.start = StartDate;
      ViewBag.end = EndDate;
      return View(lstEvents);
    }


    //
    // GET: /KultiEvents/Details/5
    public ActionResult Details(int id = 0)
    {
      Events ev;
      var lstLocations = new List<string>();
      var hlp = new HelpersViewModel();

      if (id == 0)
      { return HttpNotFound(); }
      try
      {
        ev = db.Events.Single(e => e.EventId == id);
        var agency = new AgenciesEntities();
        Agencies agencySingle =
          (from ag in agency.Agencies
           where ag.AgencyId == ev.EvAgencyId
           select ag).FirstOrDefault();

        if (agencySingle != null)
        {
          var dic = new Dictionary<string, string>();
          dic.Add("Veranstalter-Name", agencySingle.AgencyName);
          dic.Add("Veranstalter-Email", agencySingle.AgencyEmail);
          dic.Add("Veranstalter-Tel", agencySingle.AgencyPhone);
          dic.Add("Kontakt-Person", agencySingle.ContactFirstName + " " + agencySingle.ContactLastName);
          dic.Add("Kontakt-Email", agencySingle.ContactEmail);
          dic.Add("Kontakt-Tel", agencySingle.ContactPhone);
          ViewBag.AgencyDetail = dic;
        }

        foreach (var loc in hlp.GetLocations(id))
        {
          if (loc.Item3)
          { lstLocations.Add(loc.Item1); }
        }

        ViewBag.Locations = lstLocations;
        ViewBag.UserName = HlpView.GetUserName(ev.EvUserId);
      }
      catch (Exception e)
      { return HttpNotFound(); }
      return View(ev);
    }

    //
    // GET: /KultiEvents/Create
    public ActionResult Create()
    {
      var events = new Events();
      var viewModel = new EditEventViewModel
        {
          Status = HlpView.Status,
          Type = HlpView.EventType,
          Group = HlpView.Group,
          Size = HlpView.EventSize,
          Agency = HlpView.Agency,
          EventLocations = HlpView.GetLocations(0) //return all locations as false
        };
      return View(viewModel);
    }


    //
    // POST: /KultiEvents/Create
    [HttpPost]
    [ValidateInput(false)]
    public ActionResult Create(EditEventViewModel events, HttpPostedFileBase img,
                               List<string> lstLoc)
    {
      if (ModelState.IsValid)
      {

        //return View(events);
        
        try
        {
          int locations = 0;
          string targetPath = string.Empty;
          if (lstLoc != null) //location(s) selected
          {
            foreach (string item in lstLoc)
            {
              locations += Convert.ToInt16(item);
            }
          }
          ////save data
          events.CEvents.EvStatus = events.SelectedStatusValue;
          events.CEvents.EvLocation = locations;
          events.CEvents.EvType = events.SelectedTypeValue;
          events.CEvents.EvSize = events.SelectedSizeValue;
          events.CEvents.EvGroupId = events.SelectedGroupValue;
          events.CEvents.EvAgencyId = events.SelectedAgencyValue;
          events.CEvents.EvUserId = WebSecurity.GetUserId(User.Identity.Name);
          db.Events.AddObject(events.CEvents);
          db.SaveChanges();
          if (img != null)
          {
            //read height & width from web.config
            int imgWidth = Convert.ToInt16(ConfigurationManager.AppSettings["EventImgWidth"]);
            int imgHeight = Convert.ToInt16(ConfigurationManager.AppSettings["EventImgHeight"]);
            //save image & update image path-property in model
            targetPath = Server.MapPath("~/Images/Events/" + events.CEvents.EventId + ".jpg");
            Image sourceImg = Image.FromStream(img.InputStream);
            // see http://www.codeproject.com/Articles/191424/Resizing-an-Image-On-The-Fly-using-NET
            Image resized = HlpView.ResizeImage(sourceImg, new Size(imgWidth, imgHeight));
            resized.Save(targetPath, ImageFormat.Jpeg);
            Events Event = db.Events.Single(d => d.EventId == events.CEvents.EventId);
            Event.EvImgPath1 = targetPath;
            db.ObjectStateManager.ChangeObjectState(Event, EntityState.Modified);
            db.SaveChanges();
          }
          TempData["Success"] = "Der Event wurde erstellt";
          return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
          TempData["Success"] = "Fehler beim Erstellen: " + ex.Message;
          return RedirectToAction("Index");
        }
      }
      return View(events);
    }

    //
    // GET: /KultiEvents/Edit/5
    public ActionResult Edit(int id = 0)
    {
      Events events = db.Events.Single(e => e.EventId == id);

      if (events == null)
      { return HttpNotFound(); }

      var hlpView = new HelpersViewModel();
      bool isInGroup = false;
      int GrpId = events.EvGroupId ?? 0;

      if (HlpView.GetUserId(User.Identity.Name) == events.EvUserId || events.EvGroupId < 1)
      { isInGroup = true; }

      if (!isInGroup)  //make shure that current user belongs to group who created event
      {
        var roles = (SimpleRoleProvider)Roles.Provider;
        List<string> getRolesOfCurrentUser = roles.GetRolesForUser(User.Identity.Name).ToList();
        foreach (string s in getRolesOfCurrentUser)
        {
          if (HlpView.GetParamValueByPk(GrpId) == s)
          {
            isInGroup = true;
            break;
          }
        }
      }

      if (!isInGroup)
      {
        ViewBag.NotAllowed = "Du kannst den Event nicht ändern, da er von einer anderen Gruppe erstellt wurde.";
        return View("Index", db.Events.ToList());
      }

      var viewModel = new EditEventViewModel
        {
          CEvents = events,
          Status = HlpView.Status,
          Type = HlpView.EventType,
          Size = HlpView.EventSize,
          Group = HlpView.Group,
          Agency = hlpView.Agency,
          SelectedLocationValue = events.EvLocation,
          SelectedStatusValue = events.EvStatus,
          SelectedTypeValue = events.EvType,
          SelectedSizeValue = events.EvSize,
          SelectedGroupValue = events.EvGroupId,
          SelectedAgencyValue = events.EvAgencyId,
          EventLocations = HlpView.GetLocations(events.EventId),
          Docs = HlpView.LstDocsOfEvent(events.EventId)
        };

      return View(viewModel);
    }


    //
    // POST: /KultiEvents/Edit/5
    [HttpPost]
    [ValidateInput(false)]
    public ActionResult Edit(EditEventViewModel events, HttpPostedFileBase img,
                             List<string> lstLoc)
    {
      if (events.CEvents == null)
      { return View(); }

      if (ModelState.IsValid)
      {
        int locations = 0;
        Events Event = db.Events.Single(d => d.EventId == events.CEvents.EventId);
        if (Event == null)
        { return HttpNotFound(); }

        try
        {
          if (img != null)
          {
            //delete old image
            try
            { System.IO.File.Delete(events.CEvents.EvImgPath1); }
            catch {}
            //add new image
            string targetPath = Server.MapPath("~/Images/Events/" + events.CEvents.EventId + ".jpg");
            Image sourceImg = Image.FromStream(img.InputStream);
            // http://www.codeproject.com/Articles/191424/Resizing-an-Image-On-The-Fly-using-NET
            Image resized = HlpView.ResizeImage(sourceImg, new Size(320, 415));
            resized.Save(targetPath, ImageFormat.Jpeg);
            Event.EvImgPath1 = targetPath;
          }

          if (lstLoc != null) //selected locations 
          {
            foreach (string item in lstLoc)
            { locations += Convert.ToInt16(item); }
          }
          //save data
          Event.EvLocation = locations;
          Event.EvStatus = events.SelectedStatusValue;
          Event.EvType = events.SelectedTypeValue;
          Event.EvSize = events.SelectedSizeValue;
          Event.EvGroupId = events.SelectedGroupValue;
          Event.EvAgencyId = events.SelectedAgencyValue;
          Event.EvText = events.CEvents.EvText;
          if (events.CEvents.EvAdmission != null)
          { Event.EvAdmission = events.CEvents.EvAdmission.Substring(0, Math.Min(events.CEvents.EvAdmission.Length, 49)); }
          else
          { Event.EvAdmission = null; }
          Event.EvTitle = events.CEvents.EvTitle;
          Event.EvStartDate = events.CEvents.EvStartDate;
          Event.EvEndDate = events.CEvents.EvEndDate;
          Event.EvResponsible = events.CEvents.EvResponsible ?? "";
          Event.EvInterInfo = events.CEvents.EvInterInfo ?? "";
          db.ObjectStateManager.ChangeObjectState(Event, EntityState.Modified);
          db.SaveChanges();
          //end save data
          TempData["Success"] = "Der Event wurde aktualisiert";
          return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
          TempData["Success"] = "Fehler beim aktualisieren: " + ex.Message;
          return RedirectToAction("Index");
        }
      }
      return View(events);
    }

    //
    // GET: /KultiEvents/Delete/5
    public ActionResult Delete(int id = 0)
    {
      Events events = db.Events.Single(e => e.EventId == id);

      if (events == null)
      { return HttpNotFound(); }

      bool isInGroup = false;

      if (events.EvGroupId < 1)
      { isInGroup = true; }

      if (!isInGroup)   //make shure that current user belongs to group who created event
      {
        var roles = (SimpleRoleProvider)Roles.Provider;
        List<string> getRolesOfCurrentUser = roles.GetRolesForUser(User.Identity.Name).ToList();
        int GrpId = events.EvGroupId ?? 0;
        foreach (string s in getRolesOfCurrentUser)
        {
          if (HlpView.GetParamValueByPk(GrpId) == s)
          {
            isInGroup = true;
            break;
          }
        }
      }

      if (!isInGroup)
      {
        ViewBag.NotAllowed = "Du kannst den Event nicht löschen, da er von einer anderen Gruppe erstellt wurde.";
        return View("Index", db.Events.ToList());
      }
      return View(events);
    }

    //
    // POST: /KultiEvents/Delete/5
    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      try
      {
        Events events = db.Events.Single(e => e.EventId == id);
        db.Events.DeleteObject(events);
        db.SaveChanges();
        TempData["Success"] = "Der Event wurde gelöscht";
      }
      catch (Exception ex)
      { TempData["Success"] = "Fehler beim löschen: " + ex.Message; }
      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }


    //
    // helper method for json-date conversion
    private long ToUnixTimestamp(DateTime dt)
    {
      DateTime temp;
      if (DateTime.TryParse(dt.ToShortDateString(), out temp))
      {
        var unixRef = new DateTime(1970, 1, 1, 0, 0, 0);
        return (dt.Ticks - unixRef.Ticks) / 10000000;
      }
      else
      {
        return 0;
      }
      
    }
  }
}