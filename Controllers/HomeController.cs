using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mail;
using System.Web.Mvc;
using Mvc4_Kulti.Models;
using Mvc4_Kulti.ViewModels;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Xml;

namespace Mvc4_Kulti.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult About()
    {
      return View();
    }

    public ActionResult Living()
    {
      return View();
    }

    //public ActionResult Sticker()
    //{
    //  return View();
    //}

    public ActionResult PanoramaView()
    {
      return View();
    }

    //Get Contact
    public ActionResult Contact(int id)
    {
      var items = new List<SelectListItem>();
      items.Add(new SelectListItem {Text = "Bitte wählen", Value = "", Selected = true});
      items.Add(new SelectListItem {Text = "Büro", Value = "1"});
      items.Add(new SelectListItem {Text = "Buchhaltung", Value = "2"});
      items.Add(new SelectListItem {Text = "Beiz/Umbruchbar", Value = "3"});
      items.Add(new SelectListItem {Text = "Booking", Value = "4"});
      items.Add(new SelectListItem {Text = "Fundbüro", Value = "5"});
      items.Add(new SelectListItem {Text = "Koko", Value = "6"});
      items.Add(new SelectListItem {Text = "Kultihalle", Value = "7"});
      items.Add(new SelectListItem {Text = "Trägerschaft", Value = "8"});
      items.Add(new SelectListItem {Text = "Vermietung", Value = "9"});
      items[id].Selected = true;
      ViewBag.Subject = items;
      ViewBag.Showform = "true";

      var random = new Random();
      var v = new Cotcha();

      int nr1 = random.Next(10);
      int nr2 = random.Next(10);
      bool bOpreator = (DateTime.Now.Second%2) == 1;
      v.Task = bOpreator ? "+" : "-";
      if (v.Task == "+")
      {
        v.Number1 = nr1;
        v.Number2 = nr2;
      }
      else
      {
        if (nr1 >= nr2)
        {
          v.Number1 = nr1;
          v.Number2 = nr2;
        }
        else
        {
          v.Number1 = nr2;
          v.Number2 = nr1;
        }
      }
      return View(v);
    }

    //Post Contact
    [HttpPost]
    public ActionResult Contact(Cotcha reslt, string msg, string titel, string name, int subject, string contact)
    {
      ViewBag.Showform = "false";
      bool ok = false;

      switch (reslt.Task)
      {
        case "+":
          if (reslt.Number1 + reslt.Number2 == reslt.Result)
          {
            ok = true;
          }
          break;
        case "-":
          if (reslt.Number1 - reslt.Number2 == reslt.Result)
          {
            ok = true;
          }
          break;
      }

      if (ok)
      {
        string sRecipient = "info@kulturfabrik.ch";
        string sSubject = string.Empty;
        switch (subject)
        {
          case 1:
            sSubject = "Büro";
            break;
          case 2:
            sSubject = "Buchhaltung";
            sRecipient = "dominique_meili@yahoo.de";
            break;
          case 3:
            sSubject = "Beiz/Umbruchbar";
            sRecipient = "umbruchbar@kulturfabrik.ch";
            break;
          case 4:
            sSubject = "Booking";
            sRecipient = "booking@kulturfabrik.ch";
            break;
          case 5:
            sSubject = "Fundbüro";
            sRecipient = "fundbuero@kulturfabrik.ch";
            break;
          case 6:
            sSubject = "Koko";
            sRecipient = "koko@kulturfabrik.ch";
            break;
          case 7:
            sSubject = "Kultihalle";
            sRecipient = "kultihalle@kulturfabrik.ch";
            break;
          case 8:
            sSubject = "Trägerschaft";
            sRecipient = "traegerschaft@kulturfabrik.ch";
            break;
          case 9:
            sSubject = "Vermietung";
            sRecipient = "schoeni@dplanet.ch";
            break;
        }
        var m = new MailMessage();
        m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
        m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "info@kulturfabrik.ch");
        m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "inf_frm%2%14");
        m.BodyFormat = MailFormat.Html;
        m.From = "info@kulturfabrik.ch";
        m.To = sRecipient;
        m.Subject = "Homepage: Kontaktaufnahme";
        m.Body = titel + "<br/><br/>Thema: " + sSubject + "<br/><br/>" + msg.Replace("\n", "<br/>") + "<br/><br/>" +
                 "Von:<br/>" + name + "<br/>Kontakt: " + contact;
        SmtpMail.SmtpServer = "localhost";
        SmtpMail.Send(m);
        ViewBag.Message = "Danke, wir werden uns melden!";
      }
      else
      {
        ViewBag.Message = "Leider ist ein Rechnungsfehler aufgetreten..";
      }
      return View();
    }

    //GET list ListGroups (folders as Groups e.g. Kulturraum, Gewerbe..)
    public ActionResult ListGroups(string type)
    {
      var list = new List<GroupViewModel>();
      try
      {
        var Dir = new DirectoryInfo(Path.Combine(Server.MapPath("~/Groups/"), type));
        DirectoryInfo[] oDir = Dir.GetDirectories("*.*", SearchOption.AllDirectories);
        foreach (DirectoryInfo directoryInfo in oDir)
        {
          var g = new GroupViewModel();
          g.GroupName = directoryInfo.Name;
          g.GroupType = type;
          foreach (FileInfo info in directoryInfo.GetFiles())
          {
            if (info.Name.ToLower() == "shorttext.txt")
            {
              using (var objStreamReader = new StreamReader(info.FullName, Encoding.Default))
              {
                g.HtmlShortTxt = objStreamReader.ReadToEnd();
              }
              break;
            }
          }
          list.Add(g);
        }
      }
      catch (Exception)
      {
        throw;
      }
      return View(list);
    }


    //GET Group (display individual folder (Kulturraum, Gewerbe..
    public ActionResult Group(string groupType, string groupName)
    {
      var oGroupViewModel = new GroupViewModel();
      string htmlContent = string.Empty;
      var docs = new List<string>();
      try
      {
        var Dir = new DirectoryInfo(Path.Combine(Server.MapPath("~/Groups/" + groupType), groupName));
        FileInfo[] FileList = Dir.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (FileInfo FI in FileList)
        {
          if (FI.Name.ToLower() == "content.txt")
          {
            using (var objStreamReader = new StreamReader(FI.FullName, Encoding.Default))
            {
              htmlContent = objStreamReader.ReadToEnd();
            }
          }
          else if (FI.Extension.ToLower() != ".jpg" && FI.Extension.ToLower() != ".txt")
          {
            docs.Add(Path.Combine(Server.MapPath("~/Groups"), FI.Name));
          }
        }

        oGroupViewModel.GroupType = groupType;
        oGroupViewModel.GroupName = groupName;
        oGroupViewModel.HtmlContent = htmlContent;
        oGroupViewModel.Documents = docs;
      }
      catch
      {
        return View(oGroupViewModel);
      }

      return View(oGroupViewModel);
    }


    //GET Nostalgie
    public ActionResult Nostalgie()
    {
      var Root = new ClassNostalgie();
      var n = new NostalgieViewModel();
      try
      {
        var dir = new DirectoryInfo(Server.MapPath("~/Groups/Nostalgie"));
        DirectoryInfo[] oDir = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        foreach (DirectoryInfo directoryInfo in oDir)
        {
          if (directoryInfo.GetDirectories().Any())
          {
            var c = new ClassNostalgie();
            c.FolderName = directoryInfo.Name;
            c.Type = "Year";
            c.Path = directoryInfo.FullName;
            var dir2 = new DirectoryInfo(directoryInfo.FullName);
            DirectoryInfo[] oDir2 = directoryInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            foreach (DirectoryInfo info in oDir2)
            {
              var x = new ClassNostalgie();
              x.FolderName = info.Name;
              x.Type = "files";
              x.Path = info.FullName;
              c.SubFolders.Add(x);
            }
            Root.SubFolders.Add(c);
          }
        }
        Root.SubFolders.Reverse();
      }
      catch (Exception ex)
      {
        var e = new ClassNostalgie();
        e.FolderName = ex.Message;
        Root.SubFolders.Add(e);
      }

      n.ClsNos = Root;

      return View(n);
    }


   public JsonResult ViewFolder(string year, string name)
    {
         var htmlStr = new StringBuilder();
         var L = new List<string>();
         string sPath = "/Groups/Nostalgie/" + year + "/" + name + "/"; 
         try
         {
            var dir = new DirectoryInfo(Server.MapPath("~/Groups/Nostalgie/" + year + "/" + name));
            foreach (FileInfo oFlile in dir.GetFiles())
            {
               switch (oFlile.Extension.ToLower())
               {
                  case ".txt":
                     string line;
                     int cnt = 0;
                     using (System.IO.StreamReader sr = new System.IO.StreamReader(oFlile.FullName, Encoding.Default))
                     {
                        while ((line = sr.ReadLine()) != null)
                        {
                           if (cnt == 0)
                           {
                              htmlStr.Append("<h3>" + line.ToString() + "</h3>");
                              cnt++;
                           }
                           else
                           {
                              if (line.Contains("!video"))
                              {
                                 htmlStr.Append("<iframe width='425' height='240px' src='" +
                                    line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1) +
                                       "' frameborder='0' allow='autoplay; encrypted-media' allowfullscreen></iframe><br/>");
                              }
                              else if (line.Contains("!audio"))
                              {
                                 htmlStr.Append("<audio controls style='width:300px;height:30px;'" + "><source src ='"
                                          + line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1)
                                                + "'/>Your browser doesn't support HTML5 audio tag.</audio><div>" 
                                                   + line.Substring(line.LastIndexOf("/") + 1, line.Length - line.LastIndexOf("/") - 2) + "</div>");
                              }
                              else if (line.Length < 2)
                              { htmlStr.Append("<br/>"); }
                              else
                              { htmlStr.Append(line.ToString() + "<br/>"); }
                           }
                        }
                     }
                     break;
                  case ".jpg":
                  case ".mp3":
                  case ".mp4":
                     L.Add(oFlile.Name);
                     break;
                  default:
                     // skip file
                     break;
               }
            }

         }
         catch (Exception ex)
         {
            htmlStr.Append(ex.Message);
         }

         L.Insert(0,htmlStr.ToString());

         return Json(L, JsonRequestBehavior.AllowGet);
    }

        //private IEnumerable<string> doParse(StreamReader reader)
        //{
        //    string line;
        //    bool first = false;
        //    while ((line = reader.ReadLine()) != null)
        //    {
        //        if (!line.StartsWith("#"))
        //        {
        //            if (first)
        //            {
        //                yield return line;
        //            }
        //        }
        //        else if (!first)
        //        {
        //            first = true;
        //        }
        //        else
        //        {
        //            yield break;
        //        }
        //    }
        //}
   }


    public class ClassNostalgie
  {
        private List<FileInfo> files = new List<FileInfo>();
        private string folderName;
        private string path;
        private List<ClassNostalgie> subFolders = new List<ClassNostalgie>();
        private string type;

        public List<FileInfo> Files { get => files; set => files = value; }
        public string FolderName { get => folderName; set => folderName = value; }
        public string Path { get => path; set => path = value; }
        public List<ClassNostalgie> SubFolders { get => subFolders; set => subFolders = value; }
        public string Type { get => type; set => type = value; }
    }

  public class FileLst
  {
        private List<FileInfo> files = new List<FileInfo>();

        public List<FileInfo> Files { get => files; set => files = value; }
  }
}