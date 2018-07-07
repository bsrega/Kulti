using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Mvc4_Kulti.Filters;
using Mvc4_Kulti.LuceneService;
using Mvc4_Kulti.ViewModels;
//using Lucene.Net.Store;

namespace Mvc4_Kulti.Controllers
{
  [Authorize]
  [InitializeSimpleMembership]

  public class LuceneController : Controller
  {

    public ActionResult Index(string searchTerm, string searchField, bool? searchDefault, int? limit)
    {
      return View();
    }

    //GET: Search

    public ActionResult Search(string searchTerm)
    {
      if(searchTerm.Length == 0)
      { return RedirectToAction("ListAll", "Document"); }
      //remove invalid search-wildcards
      if (searchTerm.Substring(0, 1) == "*" || searchTerm.Substring(0, 1) == "?")
      {
        searchTerm = searchTerm.Substring(1, searchTerm.Length - 1);
      }
      //assure minimum lenght
      if (searchTerm.Length < 3)
      {
        ViewBag.Message = "min. 3 Buchstaben.";
        return View("../Document/ListAll");
      }
      //do search
      List<int> t = LuceneService.Lucene.SearchIndex(searchTerm).ToList();
      TempData["FoundDocs"] = t;
      TempData["SearchTerm"] = searchTerm;
      return RedirectToAction("ListAll","Document");
    }

    public ActionResult CreateIndex()
    {
      LuceneService.Lucene.CreateLuceneIndex();
      TempData["Result"] = "Search index was created successfully!";
      return RedirectToAction("Index");
    }

    public ActionResult ClearIndex()
    {
      if (LuceneService.Lucene.ClearLuceneIndex())
        TempData["Result"] = "Search index was cleared successfully!";
      else
        TempData["ResultFail"] = "Index is locked and cannot be cleared, try again later or clear manually!";
      return RedirectToAction("Index");
    }

    public ActionResult ClearIndexRecord(int id)
    { 
      LuceneService.Lucene.ClearLuceneIndexRecord(id);
      TempData["Result"] = "Search index record was deleted successfully!";
      return RedirectToAction("Index");
    }

    public ActionResult OptimizeIndex()
    {
      LuceneService.Lucene.Optimize();
      TempData["Result"] = "Search index was optimized successfully!";
      return RedirectToAction("Index");
    }

    //
    //HELPER: get username by id
    private string getUsername(int userid)
    {
      using (SqlCommand oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select UserName From UserProfile Where Userid = " + userid;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
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
  }

}