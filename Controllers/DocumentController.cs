using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Mvc4_Kulti.Filters;
using Mvc4_Kulti.ViewModels;
using WebMatrix.WebData;
using System.Web.Mail;

namespace Mvc4_Kulti.Controllers
{
  [Authorize]
  [InitializeSimpleMembership]
  public class DocumentController : Controller
  {
    private readonly HelpersViewModel HlpView = new HelpersViewModel();
    private readonly KultiDocumentEntities db = new KultiDocumentEntities();

    //
    // GET: /Document/

    public ActionResult Index()
    { return View(db.Documents.ToList()); }

    //
    // GET: /Document/getReport
    [AllowAnonymous]
    public ActionResult GetReport(int id = 0)
    {
      try
      {
        Documents doc = db.Documents.Single(d => d.DocId == id);
        //if (doc.Title.Length == 0 || doc.Type != 5 || doc.Type != 4 || doc.Type != 5 || doc.Status != 2)
        //{ return HttpNotFound(); }
        var viewModel = new DocumentSingleViewModel();
        viewModel.Doc = doc;
        viewModel.UserName = getUsername(doc.UserId);
        viewModel.DocType = getDocType(doc.Type ?? 0);
        viewModel.Group = getDocType(doc.GroupId ?? 0);
        viewModel.Status = getDocType(doc.Status ?? 0);
        viewModel.EventType = getDocEvent(doc.EventId ?? 0);
        if (viewModel.EventType.Length == 0)
        { viewModel.EventType = "kein Event"; }
        return View(viewModel);
      }
      catch (Exception)
      {
        TempData["Success"] = "Das Dokument wurde gelöscht";
        return View();
      }
    }

    //
    // GET: /Document/ListAll/
    public ActionResult ListAll()
    {
      var docList = new List<Documents>();

      if (TempData["FoundDocs"] != null)
      {
        var t = (List<int>) TempData["FoundDocs"];
        if (t.Count == 0)
        { ViewBag.message = "Suche nach <span style='color: red;'>" + TempData["SearchTerm"] + "</span> hat keine Resultate"; }
        else
        { ViewBag.message = "<span style='color: red;'>" + TempData["SearchTerm"] + "</span> kommt in folgenden Dokumenten vor:"; }

        docList = db.Documents.ToList().Where(x => t.Contains(x.DocId)).ToList();
      }
      else // get all documents, unordered
      {
        if (Request.QueryString["getAll"] == "true")
        {
          docList = db.Documents.OrderBy(e => e.CreateDate).ToList();
          ViewBag.message = "Alle Dokumente";
        }
        else
        {
          ViewBag.message = "Dokumente ab 2016";
          docList = db.Documents.Where(e => e.CreateDate.Year > 2015).ToList();  
        }
      }

      var viewModel = new DocumentViewModel
      { Docs = docList };

      return View(viewModel);
    }


    //
    // GET: /Document/ListPublishedDocs

    [AllowAnonymous]
    public ActionResult ListPublishedDocs()
    {
      var lDocs = new List<PublDocs>();
      List<Documents> lDocuments = db.Documents.ToList().Where(x => x.isPublic == true
                                                                    && x.PublishDate <= DateTime.Now).ToList();
      foreach (Documents document in lDocuments)
      {
        var oDocs = new PublDocs();
        oDocs.title = document.Title;
        oDocs.pk = document.DocId;
        oDocs.ext = document.FileExtension;
        lDocs.Add(oDocs);
      }
      return PartialView(lDocs);
    }


    //
    // GET: /Document/Details/5

    public ActionResult Details(int id = 0)
    {
      Documents doc = db.Documents.Single(d => d.DocId == id);
      if (doc.Title == null)
      { return HttpNotFound(); }
      var viewModel = new DocumentSingleViewModel();
      viewModel.Doc = doc;
      viewModel.UserName = getUsername(doc.UserId);
      viewModel.DocType = getDocType(doc.Type ?? 0);
      viewModel.Group = getDocType(doc.GroupId ?? 0);
      viewModel.Status = getDocType(doc.Status ?? 0);
      viewModel.EventType = getDocEvent(doc.EventId ?? 0);
      if (viewModel.EventType.Length == 0)
      { viewModel.EventType = "kein Event"; }
      return View(viewModel);
    }

    //
    // GET: /Document/ViewDoc/5
    [AllowAnonymous]
    public ActionResult ViewDoc(int id = 0)
    {
      try
      {
        Documents doc = db.Documents.Single(d => d.DocId == id);

        //if (doc.Title.Length == 0 || doc.Status != 2)
        //{
        //  return HttpNotFound(); 
        //}
        switch (doc.FileExtension)
        {
          case ".pdf":
            return File(doc.FilePath, "application/pdf");
          case ".txt":
            return File(doc.FilePath, "text/plain");
          case ".doc":
            return File(doc.FilePath, "application/msword");
          case ".xls":
            return File(doc.FilePath, "application/vnd.ms-excel");
          case ".ppt":
            return File(doc.FilePath, "application/vnd.ms-powerpoint");
          default:
            return View();
        }
      }
      catch
      { return HttpNotFound(); }
    }

    //
    // GET: /Document/Create

    public ActionResult Create()
    {
      var documents = new Documents();

      var viewModel = new DocumentCreateViewModel
        {
          Events = HlpView.EventsForDocs,
          Status = HlpView.Status,
          Type = HlpView.DocType,
          Group = HlpView.Group,
          Doc = documents
        };

      return View(viewModel);
    }

    //
    // POST: /Document/Create

    [HttpPost]
    public ActionResult Create(DocumentCreateViewModel documents, HttpPostedFileBase DocFile)
    { 
      string sPath = string.Empty;
      int docPK = 0;

      if (DocFile == null) //nothing to do..
      { return RedirectToAction("ListAll"); }

      try
      {
      //filter document type
      switch (Path.GetExtension(DocFile.FileName.ToLower())) 
      {
        case ".pdf":
        case ".doc":
        case ".txt":
        case ".ppt":
        case ".xls":
          break;
        default:
          TempData["WrongType"] = "Fehler: Nur PDF, DOC, XLS, PPT & TXT - Files sind zugelassen";
          return RedirectToAction("ListAll");
      }

      string fileName = Path.GetFileName(DocFile.FileName.Split('.').First()
                                         + "_" + DateTime.Now.ToShortDateString() + '.'
                                         + Path.GetFileName(DocFile.FileName.Split('.').Last()));
      if (ModelState.IsValid)
      {
        documents.Doc.Status = documents.SelectedStatusValue;
        documents.Doc.EventId = documents.SelectedEventValue;
        documents.Doc.Type = documents.SelectedTypeValue;
        documents.Doc.GroupId = documents.SelectedGroupValue;
        documents.Doc.UserId = WebSecurity.GetUserId(User.Identity.Name);
        documents.Doc.CreateDate = DateTime.Now;
        documents.Doc.FilePath = Path.Combine(Server.MapPath("~/Documents"), fileName);
        documents.Doc.FileExtension = Path.GetExtension(DocFile.FileName);
        db.Documents.AddObject(documents.Doc);
        db.SaveChanges();
        docPK = documents.Doc.DocId;
        //update filepath
        Documents doc = db.Documents.Single(d => d.DocId == docPK);
        doc.FilePath = Path.Combine(Server.MapPath("~/Documents"), docPK.ToString() + "_" + fileName);
        sPath = doc.FilePath;
        //save db
        db.SaveChanges();
        //save physical file
        DocFile.SaveAs(sPath);
        //add document to Lucene-Index
        LuceneService.Lucene._addToLuceneIndex(sPath);
        LuceneService.Lucene.Optimize();
        db.Refresh(RefreshMode.ClientWins, db.Documents);

        //send protocoll to users if doc.type is 'prototcol' & checkbox 'send protos' is checked 
        bool mailProto = Convert.ToBoolean(Request["MailProto"].Contains("true"));
        if (mailProto && documents.SelectedTypeValue == 5)
        {
          int s = SendProtos(docPK, documents.SelectedGroupValue ?? 0, fileName);
          TempData["Success"] = "Das Dokument wurde erfasst & Anzahl versendet: " + s;
          return RedirectToAction("ListAll");
                    
          //if (sendProtos(docPK, documents.SelectedGroupValue ??  0))
          //{
          //  TempData["Success"] = "Das Dokument wurde erfasst & versendet";
          //  return RedirectToAction("ListAll");
          //}
          //TempData["Success"] = "Dokument wurde erfasst, Fehler beim Email-Versand";
          //return RedirectToAction("ListAll");
        }

      }
      TempData["Success"] = "Das Dokument wurde erfasst";
      return RedirectToAction("ListAll");
      }
      catch (Exception ex)
      {
        TempData["Success"] = "Fehler beim Erfassen: " + ex.Message;
        return RedirectToAction("ListAll");
      }

    }

    //
    // GET: /Document/Edit/5

    public ActionResult Edit(int id)
    {
      bool isInGroup = false;
      Documents documents = db.Documents.Single(d => d.DocId == id);

      if (documents == null)
      { return HttpNotFound(); }

      //make shure that current user belongs to group of user who created event
      var roles = (SimpleRoleProvider)Roles.Provider;
      List<string> getRolesOfCurrentUser = roles.GetRolesForUser(User.Identity.Name).ToList();
      int GrpId = documents.GroupId ?? 0;
      foreach (var s in getRolesOfCurrentUser)
      {
        if (HlpView.GetParamValueByPk(GrpId) == s)
        {
          isInGroup = true;
          break;
        }
      }

      if (!isInGroup)
      {
        ViewBag.NotAllowed = "Du kannst das Dokument nicht ändern, da es von einer anderen Gruppe erstellt wurde.";
        return View("ListAll");
      }

      var viewModel = new DocumentEditViewModel
      {
        Doc = documents,
        Events = HlpView.EventsForDocs,
        SelectedEventValue = documents.EventId,
        Status = HlpView.Status,
        SelectedStatusValue = documents.Status,
        Type = HlpView.DocType,
        SelectedTypeValue = documents.Type,
        Group = HlpView.Group,
        SelectedGroupValue = documents.GroupId
      };
      return View(viewModel);  
    }

    //
    // POST: /Document/Edit/5

    [HttpPost]
    public ActionResult Edit(DocumentEditViewModel documents)
    {
      if (ModelState.IsValid)
      {
        Documents doc = db.Documents.Single(d => d.DocId == documents.Doc.DocId);
        doc.PublishDate = documents.Doc.PublishDate;
        doc.EventId = documents.SelectedEventValue;
        doc.Status = documents.SelectedStatusValue;
        doc.Type = documents.SelectedTypeValue;
        doc.GroupId = documents.SelectedGroupValue;
        doc.Title = documents.Doc.Title;
        doc.isPublic = documents.Doc.isPublic;
        db.SaveChanges();
        return RedirectToAction("ListAll");
      }
      return View(documents);
    }

    //
    // GET: /Document/Delete/5

    public ActionResult Delete(int id = 0)
    {
      bool isInGroup = false;
      Documents documents = db.Documents.Single(d => d.DocId == id);
      if (documents == null)
      {
        return HttpNotFound();
      }

      //make shure that current user belongs to group of user who created event
      var roles = (SimpleRoleProvider)Roles.Provider;
      List<string> getRolesOfCurrentUser = roles.GetRolesForUser(User.Identity.Name).ToList();
      int GrpId = documents.GroupId ?? 0;
      foreach (var s in getRolesOfCurrentUser)
      {
        if (HlpView.GetParamValueByPk(GrpId) == s)
        {
          isInGroup = true;
          break;
        }
      }

      if (!isInGroup)
      {
        ViewBag.NotAllowed = "Du kannst das Dokument nicht löschen, da es von einer anderen Gruppe erstellt wurde.";
        return View("ListAll");
      }

      return View(documents);
    }

    //
    // POST: /Document/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      bool isInGroup = false;

      Documents documents = db.Documents.Single(d => d.DocId == id);
      if (documents == null)
      {
        TempData["Success"] = "Fehler beim Dokument-Index";
        return RedirectToAction("ListAll");
      }

      //check that this user belongs to the group who created event
      var roles = (SimpleRoleProvider)Roles.Provider;
      List<string> getRolesOfCurrentUser = roles.GetRolesForUser(User.Identity.Name).ToList();
      int GrpId = documents.GroupId ?? 0;
      foreach (var s in getRolesOfCurrentUser)
      {
        if (HlpView.GetParamValueByPk(GrpId) == s)
        {
          isInGroup = true;
          break;
        }
      }

      if (!isInGroup)
      {
        ViewBag.NotAllowed = "Du kannst das Dokument nicht ändern, da es von einer anderen Gruppe erstellt wurde.";
        return View("ListAll");
      }
      try
      {
        //delete file
        var fi = new FileInfo(documents.FilePath);
        if (fi.Exists)
        { fi.Delete(); }
        //delete file from Lucene-index
        LuceneService.Lucene.DeleteDocumentFromIndex(id);
        //delete file-info from db
        db.Documents.DeleteObject(documents);
        db.SaveChanges();
      }
      catch (Exception ex)
      {
        TempData["Success"] = ex.Message;
        return RedirectToAction("ListAll");
      }
      TempData["Success"] = "Das Dokument wurde gelöscht";
      return RedirectToAction("ListAll");
    }


    //public ActionResult ImportDocs()
    //{
    //  int iYear = 0;
    //  int docPK = 0;
    //  string sPath = string.Empty;
    //  var dir = new DirectoryInfo(Path.GetFullPath(Server.MapPath("~/Groups/Protokolle/")));

    //  foreach (DirectoryInfo t in dir.GetDirectories()) //year
    //  {
    //    iYear = Convert.ToInt16(t.Name);
    //    foreach (DirectoryInfo te in t.GetDirectories()) //group
    //    {
    //      var dt = new DateTime(iYear, 1, 1, 1, 1, 1, 1);
    //      foreach (FileInfo f in te.GetFiles())
    //      {
    //        Documents oDoc = new Documents();
    //        oDoc.CreateDate = dt;
    //        oDoc.GroupId = Convert.ToInt16(te.Name);
    //        oDoc.Status = 2; //release
    //        oDoc.Title = f.Name;
    //        oDoc.Type = 5; //protokoll
    //        oDoc.UserId = 12; //schüfi
    //        oDoc.FileExtension = f.Extension.ToLower();
    //        oDoc.FilePath = Path.Combine(Server.MapPath("~/Documents"), f.Name);
    //        db.Documents.AddObject(oDoc);
    //        db.SaveChanges();
    //        docPK = oDoc.DocId;
    //        Documents doc = db.Documents.Single(d => d.DocId == docPK);
    //        doc.FilePath = Path.Combine(Server.MapPath("~/Documents"), docPK.ToString() + "_" + f.Name);
    //        sPath = doc.FilePath;
    //        //save db
    //        db.SaveChanges();
    //        //move physical file
    //        f.MoveTo(sPath);
    //        //add document to Lucene-Index
    //        LuceneService.Lucene._addToLuceneIndex(sPath);
    //        db.Refresh(RefreshMode.ClientWins, db.Documents);
    //      }
    //    }
    //  }
    //  return View();
    //}

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }

    //
    //HELPER: get username by id
    private string getUsername(int userid)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select UserName From UserProfile Where Userid = " + userid;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            return reader[0].ToString();
          }
          oCmd.Connection.Close();
        }
        catch (Exception ex)
        {
          return ex.Message;
        }
        return ("??");
      }
    }

    private string getDocType(int typeId)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Value From ParamCollection where id = " + typeId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            return reader[0].ToString();
          }
          oCmd.Connection.Close();
        }
        catch (Exception ex)
        {
          return ex.Message;
        }
        return ("??");
      }
    }

    private string getDocEvent(int eventId)
    {
      string sReturn = string.Empty;
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select EvTitle From Events Where EventId = " + eventId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          if (reader.HasRows)
          while (reader.Read())
          {  sReturn = reader[0].ToString(); }
          else
          { sReturn = "kein Event"; }
          oCmd.Connection.Close();
        }
        catch (Exception ex)
        {
          sReturn = ex.Message;
        }
      }
      return sReturn;
    }

    private int SendProtos(int docId, int GroupId, string docName)
    {
      string sGroupName = string.Empty;
      var oLst = HlpView.UsersEmailInGroup(GroupId);
      var extList = new List<string>();
      int iCntSentMails = 0;

      switch (GroupId)
      {
        case 12:
          sGroupName = "Prot_Traeve";
          break;
        case 33:
          sGroupName = "Prot_Traeve";
          break;
        case 11:
          sGroupName = "Prot_Koko";
          break;
        case 8:
          sGroupName = "Prot_Halle";
          break;
        case 9:
          sGroupName = "Prot_Beiz";
          break;
      }

      //get list of external users 
      if (sGroupName.Length > 0) //group is found in table external users
      {
        string sSQL = "SELECT email FROM externalusers Where " + sGroupName + " = 'x'";
        using (var oCmd = new SqlCommand())
        {
          oCmd.CommandText = sSQL;
          oCmd.CommandType = CommandType.Text;
          oCmd.Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
          try
          {
            oCmd.Connection.Open();
            SqlDataReader reader = oCmd.ExecuteReader();
            while (reader.Read())
            {
              if (!oLst.Contains(reader[0].ToString()))  //avoid duplicates
              { extList.Add(reader[0].ToString()); }
            }
            oCmd.Connection.Close();
          }
          catch
          { }
        }
      }

      //send emails
      string link = "http://kulturfabrik.ch/Document/GetReport/" + docId.ToString();
      MailMessage m = new MailMessage();
      SmtpMail.SmtpServer = "localhost";
      m.BodyFormat = MailFormat.Html;
      m.From = "protokolle@kulturfabrik.ch";
      m.Subject = "Neues Protokoll der Kulturfabrik";
      m.Body = "Hallo KultianerIn, hier der Link zum Protokoll: '" + docName + "':<br/><br/>";
      m.Body += "<a href='" + link + "' target='_new'>kulturfabrik.ch/Document/GetReport/</a>";
      m.Body += "<br/><br/>Herzlicher Gruss<br/>Kulturfabrik Wetzikon";
      m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
      m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "protokolle@kulturfabrik.ch");
      m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "p_Rot2013");

      foreach (var t in oLst)
      {
        m.To = t;
        SmtpMail.Send(m);
      }

      foreach (var s in extList)
      {
        m.To = s;
        SmtpMail.Send(m);
      }
      
      iCntSentMails = oLst.Count + extList.Count;
      return iCntSentMails;
    }
 
  }

  public class PublDocs
  {
    public string title { get; set; }
    public int pk { get; set; }
    public string ext { get; set; }
  }

}