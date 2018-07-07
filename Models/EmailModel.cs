using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;


namespace Mvc4_Kulti.Models
{
  public class EmailInfo
  {
    [Required]
    public string Subject { get; set; }
    [Required]
    public string Body { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public IEnumerable<SelectListItem> ToReceivers { get; set; }
    public IEnumerable<SelectListItem> ToRoles { get; set; }
    public int? SelectedRoleValue { get; set; }
    public HttpPostedFileBase Attachment { get; set; }
  }

  public class Cotcha
  {
    public int Number1 { get; set; }
    public int Number2 { get; set; }
    public string Task { get; set; }
    [Required]
    public int Result { get; set; }
  }

}
