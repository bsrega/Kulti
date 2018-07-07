using System.Web;
using System.Web.Mvc;
using Mvc4_Kulti.Filters;

namespace Mvc4_Kulti
{
  public class FilterConfig
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
      filters.Add(new InitializeSimpleMembershipAttribute());
      filters.Add(new HandleErrorAttribute());

    }
  }
}