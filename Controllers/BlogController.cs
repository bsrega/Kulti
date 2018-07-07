using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Mvc4_Kulti.ViewModels;
using WebMatrix.WebData;

namespace Mvc4_Kulti.Controllers
{
  public class BlogController : Controller
  {
    private readonly BlogEntities db = new BlogEntities();
    private readonly HelpersViewModel HlpView = new HelpersViewModel();
    //
    // GET: /Blog/

    public ActionResult Index()
    {
      return View(db.Blog.ToList());
    }

    //
    // GET: /Blog/ListBlogs

    public ActionResult ListBlogs()
    {
      var hlpView = new HelpersViewModel();
      return PartialView(hlpView.GetBlogs());
    }

    //
    // GET: /Blog/Details/5

    public ActionResult Details(int id = 0)
    {
      Blog blog = db.Blog.Single(b => b.BlogId == id);
      if (blog == null)
      {
        return HttpNotFound();
      }
      ViewBag.UserName = HlpView.GetUserName(blog.UserId);
      return View(blog);
    }

    //
    // GET: /Blog/Create

    public ActionResult Create()
    {
      return View();
    }

    //
    // POST: /Blog/Create

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult Create(Blog blog)
    {
      if (ModelState.IsValid)
      {
        blog.Date = DateTime.Now;
        blog.UserId = WebSecurity.GetUserId(User.Identity.Name);
        db.Blog.AddObject(blog);
        db.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(blog);
    }

    //
    // GET: /Blog/Edit/5

    public ActionResult Edit(int id = 0)
    {
      Blog blog = db.Blog.Single(b => b.BlogId == id);
      if (blog == null)
      {
        return HttpNotFound();
      }
      return View(blog);
    }

    //
    // POST: /Blog/Edit/5

    [HttpPost]
    public ActionResult Edit(Blog blog)
    {
      if (ModelState.IsValid)
      {
        db.Blog.Attach(blog);
        db.ObjectStateManager.ChangeObjectState(blog, EntityState.Modified);
        db.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(blog);
    }

    //
    // GET: /Blog/Delete/5

    public ActionResult Delete(int id = 0)
    {
      Blog blog = db.Blog.Single(b => b.BlogId == id);
      if (blog == null)
      {
        return HttpNotFound();
      }
      return View(blog);
    }

    //
    // POST: /Blog/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      Blog blog = db.Blog.Single(b => b.BlogId == id);
      db.Blog.DeleteObject(blog);
      db.SaveChanges();
      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }

  public class ListBlogs
  {
    public int? User { get; set; }
    public string BlogTitle { get; set; }
    public DateTime? BlogDate { get; set; }
    public string BlogText { get; set; }
  }

}