using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Mvc4_Kulti.Filters;
using WebMatrix.WebData;
using System.Web.Mail;
using Mvc4_Kulti.ViewModels;
using System.Data.SqlClient;

namespace Mvc4_Kulti.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]

    public class ForumController : Controller
    {
        private readonly ForumEntities db = new ForumEntities();
        private readonly List<ForumPosts> op = new List<ForumPosts>();
        private readonly HelpersViewModel HlpView = new HelpersViewModel();

        // Special properties for treeview
        public bool Selectable { get; set; }

        public bool HasChild
        {
            get
            {
                List<ForumPosts> listOfCategories = op;
                return listOfCategories.Any(c => c.Id.Equals(c.ParentId));
            }
        }


        //
        // GET: /Forum/ListAll
        public ActionResult Index()
        {
            var topics = new List<KeyValuePair<string, int>>();
            var tl = new List<ForumTopic>();

            //get topics from param-collection
            using (var oCmd = new SqlCommand())
            {
                oCmd.CommandText = "Select Value, EnumValue From ParamCollection Where Type = 'ForumTopic';";
                oCmd.CommandType = CommandType.Text;
                oCmd.Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                try
                {
                    oCmd.Connection.Open();
                    SqlDataReader reader = oCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        topics.Add(new KeyValuePair<string, int>(reader[0].ToString(), Convert.ToInt16(reader[1].ToString())));
                    }
                    oCmd.Connection.Close();
                }
                catch
                {
                    //return "";
                }
            }
            //End get topics from param-collection

            foreach (var s in topics)
            {
                using (var oCmd = new SqlCommand())
                {
                    oCmd.CommandText = "Select Count(*) as l, Max(postdate) as maxDate, Min(postdate) As minDate From Forum ";
                    oCmd.CommandText += "Where TopicId = " + s.Value;
                    oCmd.CommandType = CommandType.Text;
                    oCmd.Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    try
                    {
                        oCmd.Connection.Open();
                        SqlDataReader reader = oCmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var t = new ForumTopic();
                            t.PostsCount = Convert.ToInt16(reader[0].ToString());
                            t.LatestPostDate = reader[1].ToString();
                            t.FirstPostDate = reader[2].ToString();
                            t.Title = s.Key;
                            t.Id = s.Value;
                            tl.Add(t);
                        }
                        oCmd.Connection.Close();
                    }
                    catch
                    { }
                }
            }

            ViewData["ForumTopicList"] = tl;
            return View();
        }

        public ActionResult ViewTopic(int topicId, string topicName)
        {
            var l = new List<Mvc4_Kulti.Forum>();
            ViewBag.topicName = topicName;
            ViewBag.topicId = topicId;
            l = db.Forum.Where(e => e.TopicId == topicId).ToList();
            l.Reverse();
            return View(l);
            //return View(db.Forum.Where(e => e.TopicId == topicId).ToList());
        }

        public ActionResult IndexAdmin()
        {
            return View(db.Forum.ToList());
        }

        //json, get single post
        public ActionResult PartialView(string data)
        {
            if (data.Length > 0)
            {
                int id = Convert.ToInt32(data);
                {
                    Forum forum = db.Forum.Single(f => f.ForumId == id);
                    if (forum.UserId == WebSecurity.GetUserId(User.Identity.Name))
                    {
                        bool exists = db.Forum.Any(t => t.ParentId == id);

                        if (!exists)
                        { ViewBag.isDeletable = "true"; }
                    }
                    return PartialView("ForumPost", forum);
                }
            }
            return PartialView("ForumPost");
        }

        //json get single post admin
        public ActionResult PartialViewAdmin(string data)
        {
            if (data.Length > 0)
            {
                int id = Convert.ToInt32(data);
                {
                    Forum forum = db.Forum.Single(f => f.ForumId == id);
                    return PartialView("ForumPostAdmin", forum);
                }
            }
            return PartialView("ForumPostAdmin");
        }


        public JsonResult ListAll()
        {
            foreach (Forum f in db.Forum)
            {
                var p = new ForumPosts();
                p.Id = f.ForumId;
                p.Msg = f.Msg;
                p.Email = f.UserEmail;
                p.ParentId = f.ParentId;
                p.PostDate = f.PostDate;
                p.Title = f.Title;
                p.TopicId = f.TopicId;
                op.Add(p);
            }
            return Json(op, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Forum/Details/5
        public ActionResult Details(int id = 0)
        {
            Forum forum = db.Forum.Single(f => f.ForumId == id);
            if (forum == null)
            { return HttpNotFound(); }
            return View(forum);
        }

        //
        // GET: /Forum/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Forum/Create
        [HttpPost]
        public ActionResult Create(Forum forum, string newTitle, string newMsg, int topicId)
        {
            if (ModelState.IsValid)
            {
                Forum f = new Forum();
                f.TopicId = topicId;
                f.Msg = newMsg.Replace("\n", "<br/>");
                f.Title = newTitle;
                if (forum.ForumId > 0)
                {
                    f.ParentId = forum.ForumId;
                    var fr = db.Forum.Single(i => i.ForumId == forum.ForumId);
                    if (fr.UserId != WebSecurity.GetUserId(User.Identity.Name))
                    {
                        SendMail(WebSecurity.CurrentUserName, HlpView.GetUserEmail(fr.UserId), topicId.ToString(),
                                                      HlpView.GetForumTopicName(topicId), fr.Title, newTitle);
                    }
                }
                f.PostDate = System.DateTime.Now;
                f.UserId = WebSecurity.GetUserId(User.Identity.Name);
                f.UserName = User.Identity.Name;

                //Request von Jaromir Ott
                //if (f.ParentId == null && f.TopicId == 4 && WebSecurity.GetUserId(User.Identity.Name) != 209)
                //{
                //  TempData["PostMsg"] = "Dein Eintrag wurde nicht geposted, Du kannst bei diesem Thema nur Antworten!";
                //  return RedirectToAction("ViewTopic", new { topicId = forum.TopicId, topicName = HlpView.GetForumTopicName(topicId) });
                //}
                //End Request von Jaromir Ott

                db.Forum.AddObject(f);
                db.SaveChanges();
                TempData["PostMsg"] = "Dein Eintrag wurde geposted";
                return RedirectToAction("ViewTopic", new { topicId = forum.TopicId, topicName = HlpView.GetForumTopicName(topicId) });
            }

            return View(forum);
        }

        //
        // GET: /Forum/Edit/5
        public ActionResult Edit(int id = 0)
        {
            Forum forum = db.Forum.Single(f => f.ForumId == id);
            if (forum == null)
            { return HttpNotFound(); }
            return View(forum);
        }

        //
        // POST: /Forum/Edit/5
        [HttpPost]
        public ActionResult Edit(Forum forum)
        {
            if (ModelState.IsValid)
            {
                db.Forum.Attach(forum);
                db.ObjectStateManager.ChangeObjectState(forum, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(forum);
        }


        //GET: /Forum/DeleteOwnPost/5
        //public ActionResult DeleteOwnPost(int id = 0)
        //{
        //    Forum forum = db.Forum.Single(f => f.ForumId == id);
        //    if (forum == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(forum);
        //}

        //
        // GET: /Forum/DeleteOwnPost/5
        public ActionResult DeleteOwnPost(int id)
        {
            Forum forum = db.Forum.Single(f => f.ForumId == id);
            db.Forum.DeleteObject(forum);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /Forum/Delete/5
        public ActionResult Delete(int id = 0)
        {
            Forum forum = db.Forum.Single(f => f.ForumId == id);
            if (forum == null)
            {
                return HttpNotFound();
            }
            return View(forum);
        }

        //
        // POST: /Forum/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Forum forum = db.Forum.Single(f => f.ForumId == id);

            db.Forum.DeleteObject(forum);
            db.SaveChanges();
            DeleteChildren(id);
            return RedirectToAction("IndexAdmin");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        //Post: Delete children of deleted post
        private void DeleteChildren(int id)
        {
            List<Forum> oF = db.Forum.Where(f => f.ParentId == id).ToList();
            foreach (var forum in oF)
            {
                db.Forum.DeleteObject(forum);
                db.SaveChanges();
                DeleteChildren(forum.ForumId);
            }
        }

        //helpers
        private void SendMail(string SourceName, string sendToEmail, string topicId, string topicName, string sourceTitle, string newTitle)
        {
            try
            {
                string link = "http://kulturfabrik.ch/Forum/ViewTopic?topicId=" + topicId + "&topicName=" + topicName;
                MailMessage m = new MailMessage();
                m.BodyFormat = MailFormat.Html;
                m.From = "forum@kulturfabrik.ch";
                m.Subject = "Antwort erhalten auf dein Forum-Eintrag bei Kulti.ch";
                m.Body = "Auf dein Forum-Eintrag '" + sourceTitle + "' wurde geantwortet von " + SourceName +
                         ", mit dem Titel '" + newTitle + "'<br/><br/>Link zum Forum:<br/>";
                m.Body += "<a href='" + link + "'>" + link + "</a>";
                m.Body += "<br/><br/>Herzlicher Gruss<br/>Kulturfabrik Wetzikon";
                m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
                m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "forum@kulturfabrik.ch");
                m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "p_frm2014");
                m.To = sendToEmail;
                SmtpMail.SmtpServer = "localhost";
                SmtpMail.Send(m);
            }
            catch
            { }
        }

    }

    public class ForumPosts
    {
        public int Id { set; get; }
        public int? ParentId { set; get; }
        public int UserId { set; get; }
        public int? TopicId { set; get; }
        public string Title { set; get; }
        public string Msg { set; get; }
        public DateTime? PostDate { set; get; }
        public string Email { set; get; }
    }

    public class ForumTopic
    {
        public int Id { set; get; }
        public string Title { set; get; }
        public int PostsCount { set; get; }
        public string LatestPostDate { set; get; }
        public string FirstPostDate { set; get; }
    }

}