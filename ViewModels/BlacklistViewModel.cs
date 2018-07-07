using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Mvc4_Kulti.ViewModels
{

  public class BlacklistViewModel
  {
    public int Pk { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Reason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

  }

}