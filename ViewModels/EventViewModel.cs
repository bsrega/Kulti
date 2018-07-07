using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Mvc4_Kulti.ViewModels
{
  public class ExtEvent : Events
  {
    public List<Tuple<string, int, bool>> EventLocations { get; set; }
  }

  //public class CreateEvent
  //{
  //  public Events Events { get; set; }
  //  public string EvTitle { get; set; }
  //  public DateTime EvStartDate { get; set; }
  //  public DateTime EvEndDate { get; set; }
  //  public string EvImgPath1 { get; set; }
  //  public string EvText { get; set; }
  //  public string EvAdmission { get; set; }
  //  public int? EventId { get; set; }
  //  public int? EvLocation { get; set; }
  //  public int? EvStatus { get; set; }
  //  public int? EvUserId { get; set; }

  //  public IEnumerable<SelectListItem> Status { get; set; }
  //  public IEnumerable<SelectListItem> Type { get; set; }
  //  public int? SelectedLocationValue { get; set; }
  //  public int? SelectedStatusValue { get; set; }
  //  public int? SelectedTypeValue { get; set; }
  //  public List<Tuple<string, int, bool>> EventLocations { get; set; }

  //}


  public class EditEventViewModel
  {
    public Events CEvents { get; set; }
    
    public IEnumerable<SelectListItem> Status { get; set; }
    public IEnumerable<SelectListItem> Type { get; set; }
    public IEnumerable<SelectListItem> Group { get; set; }
    public IEnumerable<SelectListItem> Size { get; set; }
    public IEnumerable<SelectListItem> Agency { get; set; }
    public int? SelectedLocationValue { get; set; }
    public int? SelectedStatusValue { get; set; }
    public int? SelectedTypeValue { get; set; } 
    public int? SelectedGroupValue { get; set; }
    public int? SelectedAgencyValue { get; set; }
    public int? SelectedSizeValue { get; set; }
    public List<Tuple<string, int, bool>> EventLocations { get; set; }
    public List<Documents> Docs { get; set; }
  }

  public class DetailEventViewModel
  {
    public string Title { get; set; }
    public string Type { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string User { get; set; }
    public string Status { get; set; }
    public List<string> Locations  { get; set; }
    public List<string> Documents { get; set; }
    public string Image { get; set; }
    public string Admission { get; set; }
    public List<Tuple<string, int, bool>> EventLocations { get; set; }

  }

  public class ListEventsPrint
  {
    public List<Tuple<string, string, string, string>> LstEvents { get; set; }
  }


  public class ListMeetings
  {
    public List<Tuple<Events, string>> LstMeetings { get; set; }
  }





}