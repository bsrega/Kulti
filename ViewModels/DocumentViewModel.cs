using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc4_Kulti.Controllers;
using Mvc4_Kulti.Models;
using WebMatrix.WebData;

namespace Mvc4_Kulti.ViewModels
{
  
  public class DocumentViewModel
  {
    public  List<Documents> Docs  { get; set; }
  }

  public class DocumentEditViewModel
  {
    public Documents Doc { get; set; } 
    public IEnumerable<SelectListItem> Events { get; set; }
    public IEnumerable<SelectListItem> Status { get; set; }
    public IEnumerable<SelectListItem> Type { get; set; }
    public IEnumerable<SelectListItem> Group { get; set; }
    public int? SelectedEventValue { get; set; }
    public int? SelectedStatusValue { get; set; }
    public int? SelectedTypeValue { get; set; }
    public int? SelectedGroupValue { get; set; }
  }

  public class DocumentCreateViewModel
  {
    public Documents Doc { get; set; }
    public IEnumerable<SelectListItem> Events { get; set; }
    public IEnumerable<SelectListItem> Status { get; set; }
    public IEnumerable<SelectListItem> Type { get; set; }
    public IEnumerable<SelectListItem> Group { get; set; }
    public int? SelectedEventValue { get; set; }
    public int? SelectedStatusValue { get; set; }
    public int? SelectedTypeValue { get; set; }
    public int? SelectedGroupValue { get; set; }
  }

  public class DocumentSingleViewModel
  {
    public Documents Doc { get; set; }
    public string UserName { get; set; }
    public string DocType { get; set; }
    public string EventType { get; set; }
    public string Status { get; set; }
    public string Group { get; set; }
  }

}