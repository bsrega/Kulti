using System;
using System.Web.Mvc;

namespace Mvc4_Kulti.Controllers
{
  public class ErrorController : Controller
  {
    //
    // GET: /Error/
    public ActionResult Index(int statusCode, Exception exception)
    {
      Response.StatusCode = statusCode;

      ViewBag.Msg = exception.Message.ToString();

      //switch (statusCode)
      //{
      //  case 404:
      //    ViewBag.Msg = "Seite wurde nicht gefunden.";
      //    break;
      //  case 500:
      //    ViewBag.Msg = "Ein Fehler ist aufgetreten.";
      //    break;
      //  default:
      //    ViewBag.Msg = "Oops!! Fehler..";
      //    break;
      //}

      return View();
    }
  }
}