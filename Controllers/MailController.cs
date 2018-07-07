using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mail;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using Mvc4_Kulti.Filters;
using Mvc4_Kulti.Models;
using Mvc4_Kulti.ViewModels;
using WebMatrix.WebData;
using MailMessage = System.Web.Mail.MailMessage;

namespace Mvc4_Kulti.Controllers
{
  [Authorize]
  [InitializeSimpleMembership]

  public class MailController : Controller
  {
    [HttpGet]
    public ActionResult SendMail()
    {
      var hlpView = new HelpersViewModel();
      var ddf = new EmailInfo();
      ddf.ToRoles = hlpView.AllGroups.ToList();
      ddf.ToReceivers = hlpView.UsersNameAll();
      ddf.FromEmail = hlpView.GetUserEmail(WebSecurity.CurrentUserId);
      ddf.FromName = WebSecurity.CurrentUserName;
      return View(ddf);
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult SendMail(EmailInfo model, List<string> lstUsers)
    {
      if (model.Body == null || model.Body.Length < 10)
      {
        ViewData.Add("msg", "Es wurde keine Nachricht versendet, da der Text < 10 Buchstaben ist.");
        return View();
      }

      string body = "Gesendet von " + model.FromName + "<br/><br/>Nachricht:<br/>" + model.Body;
      body += "<br/><br/><hr>Antworte nicht direkt, sondern an den Sender:<br/>";
      body += model.FromEmail + "<br/><br/>Herzlicher Gruss, Kulturfabrik Wetzikon";

      MailMessage m = new MailMessage();
      SmtpMail.SmtpServer = "localhost";
      var hlpView = new HelpersViewModel();
      int iCnt = 0;

      m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
      m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "mailer@kulturfabrik.ch");
      m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "m_frm2014");
      m.BodyFormat = MailFormat.Html;
      m.From = "mailer@kulturfabrik.ch";
      m.Subject = model.Subject;
      m.Body = body;

      //attachment
      if (model.Attachment != null && model.Attachment.ContentLength > 0)
      {
        if (model.Attachment.ContentLength > 5900000)
        {
          ViewData.Add("msg", "Error: Das Attachment muss kleiner als 5MB sein!");
          return View();
        }
        try
        {
          string filename = Path.GetFileName(model.Attachment.FileName); //model.Attachment.FileName;
          model.Attachment.SaveAs(Server.MapPath("~/tempFiles/" + filename));
          MailAttachment attachment = new MailAttachment(Server.MapPath("~/tempFiles/" + filename));
          m.Attachments.Add(attachment);
        }
        catch (Exception ex)
        {
          ViewData.Add("msg", ex.Message);
          throw;
        }
      }

      try
      {
        if (lstUsers != null) //user(s) selected
        {
          foreach (string item in lstUsers)
          {
            m.To = item;
            SmtpMail.Send(m);
            iCnt += 1;
          }
        }
        else
        {
          if (model.SelectedRoleValue == 10000) //send mail to all
          {
            foreach (var s in hlpView.UsersEmailAll())
            {
              m.To = s;
              try
              {
                SmtpMail.Send(m);
                iCnt += 1;
              }
              catch
              { }
            }
          }
          else //group selected
          {
            foreach (var s in hlpView.UsersEmailInGroup(model.SelectedRoleValue ?? 0))
            {
              m.To = s;
              SmtpMail.Send(m);
              iCnt += 1;
            }
          }
        }

        ViewData.Add("msg", iCnt + " Mail(s) wurden versendet.");
      }
      catch(Exception ex)
      {
        ViewData.Add("msg", iCnt + " Mail(s) wurden versendet. Problem: " + ex.Message);
      }

        return View();
    }
  }
}
