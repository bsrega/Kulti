using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Mvc4_Kulti.Controllers;
using System.Web.Caching;

namespace Mvc4_Kulti
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : System.Web.HttpApplication
  {
    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      WebApiConfig.Register(GlobalConfiguration.Configuration);
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      //BundleConfig.RegisterBundles(BundleTable.Bundles);
      AuthConfig.RegisterAuth();
      var ci = new CultureInfo("de-CH");
      System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
      System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
    }

    //s.brun to prevent serving desktop-view instead of mobile-view --> HomeController.cs, public ActionResult Index()
    //public override string GetVaryByCustomString(HttpContext context, string custom)
    //{
    //  if (string.Equals(custom, "isMobileDevice", StringComparison.OrdinalIgnoreCase))
    //    return context.Request.Browser.IsMobileDevice.ToString();
    //  return base.GetVaryByCustomString(context, custom);
    //}

    //protected void Application_Error(object sender, EventArgs e)
    //{
    //  Exception exception = Server.GetLastError();
    //  Server.ClearError();

    //  RouteData routeData = new RouteData();
    //  routeData.Values.Add("controller", "Error");
    //  routeData.Values.Add("action", "Index");
    //  routeData.Values.Add("exception", exception);

    //  if (exception.GetType() == typeof(HttpException))
    //  {
    //    routeData.Values.Add("statusCode", ((HttpException)exception).GetHttpCode());
    //  }
    //  else
    //  {
    //    routeData.Values.Add("statusCode", 500);
    //  }

    //  IController controller = new ErrorController();
    //  controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
    //}

  }
}