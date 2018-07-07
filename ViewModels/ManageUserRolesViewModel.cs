using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;

namespace Mvc4_Kulti.ViewModels
{
  public class ManageUserRolesViewModel
  {
    public List<string> AllRoles { get; set; }
    public List<string> UserHasRoles { get; set; }
    public string UserName { get; set; }
    public string UpdateResult { get; set; }
  }

  public class ListAllUsersViewModel
  {
    //public List<string> Users { get; set; }
    public List<Tuple<string, string>> Users2 { get; set; }
  }




}