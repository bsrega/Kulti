using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Mvc4_Kulti.Controllers;

namespace Mvc4_Kulti.ViewModels
{
  public class GroupViewModel
  {
    public string GroupType { get; set; }
    public string GroupName { get; set; }
    public string HtmlContent { get; set; }
    public string HtmlShortTxt { get; set; }
    public List<string> Documents { get; set; }
  }

  public class NostalgieViewModel
  {
    public ClassNostalgie ClsNos { get; set; }
  }

  public class NostalgieFolderViewModel
  {
    public string FolderName { get; set; }
    public List<FileInfo> FileList { get; set; }

  }




}