using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Mvc4_Kulti.Filters;
using WebMatrix.WebData;

namespace Mvc4_Kulti.ViewModels
{
  [Authorize]
  [InitializeSimpleMembership]
  public class HelpersViewModel
  {
    public List<SelectListItem> Status
    {
      get { return lstDocStatus(); }
    }

    public List<SelectListItem> Events
    {
      get { return lstEvents(); }
    }

    public List<SelectListItem> EventsForDocs
    {
      get { return lstEventsForDocs(); }
    }

    public List<SelectListItem> DocType
    {
      get { return lstDocType(); }
    }

    public List<SelectListItem> EventSize
    {
      get { return lstEventSize(); }
    }

    public List<SelectListItem> EventType
    {
      get { return lstEventType(); }
    }

    public List<SelectListItem> EventLocation
    {
      get { return lstEventLocation(); }
    }

    public List<SelectListItem> Group
    {
      get { return lstGroup(); }
    }

    public List<SelectListItem> AllGroups
    {
      get { return lstAllGroups(); }
    }

    public List<SelectListItem> AllUsersName
    {
      get { return AllUsersName; }
    }

    public List<SelectListItem> Agency
    {
      get { return lstAgency(); }
    }

    public List<string> UsersEmailInGroup(int roleId)
    {
      if (roleId == 10000) //send to all users
      {
        var l = new List<string>();
        using (var oCmd = new SqlCommand())
        {
          oCmd.CommandText = "Select Email FROM UserProfile";
          oCmd.CommandType = CommandType.Text;
          oCmd.Connection =
            new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
          try
          {
            oCmd.Connection.Open();
            SqlDataReader reader = oCmd.ExecuteReader();
            while (reader.Read())
            {
              l.Add(reader[0].ToString());
            }
            oCmd.Connection.Close();
          }
          catch(Exception e)
          {}
        }
        return l;
      }

      return lstUsersEmailInGroup(roleId); //send to selected role
    }

    //public List<string> UsersEmailAll()
    //{
    //  var l = new List<string>();
    //  using (var oCmd = new SqlCommand())
    //  {
    //    oCmd.CommandText = "Select Email FROM UserProfile";
    //    oCmd.CommandType = CommandType.Text;
    //    oCmd.Connection =
    //      new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
    //    try
    //    {
    //      oCmd.Connection.Open();
    //      SqlDataReader reader = oCmd.ExecuteReader();
    //      while (reader.Read())
    //      { l.Add(reader[0].ToString()); }
    //      oCmd.Connection.Close();
    //    }
    //    catch
    //    { }
    //  }
    //  return l;
    //}

    //public string GetRoleName(int id)
    //{
    //  using (var oCmd = new SqlCommand())
    //  {
    //    oCmd.CommandText = "SELECT RoleName FROM webpages_Roles WHERE RoleId = " + id;
    //    oCmd.CommandType = CommandType.Text;
    //    oCmd.Connection =
    //      new SqlConnection(
    //        WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
    //    try
    //    {
    //      oCmd.Connection.Open();
    //      SqlDataReader reader = oCmd.ExecuteReader();
    //      while (reader.Read())
    //      {
    //        return reader[0].ToString();
    //      }
    //      oCmd.Connection.Close();
    //    }
    //    catch
    //    {
    //      return "??";
    //    }
    //  }
    //  return "Error!";
    //}

    public string GetUserName(int? userId)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "SELECT UserName From UserProfile Where UserId = " + userId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
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
        catch
        {
          return "??";
        }
      }
      return "Error!";
    }

    public string GetEventSize(int eventSizeId)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Value From ParamCollection Where id = " + eventSizeId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          { return reader[0].ToString(); }
          oCmd.Connection.Close();
        }
        catch
        { return "??"; }
      }
      return "Error!";
    }

    public string GetEventGroup(int eventGroupId)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Value From ParamCollection Where id = " + eventGroupId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          { return reader[0].ToString(); }
          oCmd.Connection.Close();
        }
        catch
        { return "??"; }
      }
      return "Error!";
    }

    public string GetForumTopicName(int topicId)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Value From ParamCollection Where enumValue = " + topicId + " And Type = 'ForumTopic';";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          { return reader[0].ToString(); }
          oCmd.Connection.Close();
        }
        catch
        { return "??"; }
      }
      return "Error!";
    }

    public int GetUserId(string username)
    {
      using (var oCmd = new SqlCommand())
      {
        try
        {
        oCmd.CommandText = "SELECT UserId From UserProfile Where UserName = '" + username.Trim() + "'";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        oCmd.Connection.Open();
        SqlDataReader reader = oCmd.ExecuteReader();
        while (reader.Read())
        { return Convert.ToInt16(reader[0]); }
        oCmd.Connection.Close();
        }
        catch
        { return 0; }
      }
      return 0;
    }

    public string GetUserEmail(int? userId)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "SELECT Email From UserProfile Where UserId = " + userId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
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
        catch
        {
          return "??";
        }
      }
      return "Error!";
    }

    public string GetParamValueByPk(int pk)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "SELECT Value From ParamCollection Where id = " + pk;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
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
        catch
        {
          return "??";
        }
      }
      return "Error!";

    }

    public Image ResizeImage(Image image, Size size,
                             bool preserveAspectRatio = true)
    {
      int newWidth;
      int newHeight;
      if (preserveAspectRatio)
      {
        int originalWidth = image.Width;
        int originalHeight = image.Height;
        float percentWidth = size.Width/(float) originalWidth;
        float percentHeight = size.Height/(float) originalHeight;
        float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
        newWidth = (int) (originalWidth*percent);
        newHeight = (int) (originalHeight*percent);
      }
      else
      {
        newWidth = size.Width;
        newHeight = size.Height;
      }
      Image newImage = new Bitmap(newWidth, newHeight);
      using (Graphics graphicsHandle = Graphics.FromImage(newImage))
      {
        graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
      }
      return newImage;
    }

    private List<string> lstUsersEmailInGroup(int roleId)
    {
      var oList = new List<string>();
      //get RoleId of Membership-Provider by RoleId from Params
      using (var oCmd = new SqlCommand())
      {
        string inSql = "SELECT UserProfile.Email FROM webpages_UsersInRoles INNER JOIN ";
        inSql += "UserProfile ON webpages_UsersInRoles.UserId = UserProfile.UserId INNER JOIN ";
        inSql += "webpages_Roles ON webpages_UsersInRoles.RoleId = webpages_Roles.RoleId ";
        inSql += "WHERE (webpages_Roles.RoleName = (SELECT Value ";
        inSql += "FROM ParamCollection WHERE (id = " + roleId + ")))";
        oCmd.CommandText = inSql;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          { oList.Add(reader[0].ToString()); }
          oCmd.Connection.Close();
        }
        catch
        { }
        return oList;
      }
    }

    public List<string> UsersEmailAll()  //exept users without any role
    {
      var oList = new List<string>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Email From UserProfile Where UserId In (Select Distinct UserId From webpages_UsersInRoles)";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          { oList.Add(reader[0].ToString()); }
          oCmd.Connection.Close();
        }
        catch
        { }
        return oList;
      }
    }

    public List<SelectListItem> UsersNameAll()  //exept users without any role
        {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Email, UserName From UserProfile Where UserId In (Select Distinct UserId From webpages_UsersInRoles) Order By UserName";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var t = new SelectListItem();
            t.Value = reader[0].ToString();
            t.Text = reader[1].ToString();
            r.Add(t);
          }
          oCmd.Connection.Close();
        }
        catch(Exception ex)
        {
          var e = new SelectListItem();
          e.Value = "";
          e.Text = ex.Message;
          r.Add(e);
        }
        return r;
      }
    }

    private List<SelectListItem> lstGroup()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        string inSql = "SELECT r.RoleName FROM webpages_Roles AS r INNER JOIN ";
        inSql += "webpages_UsersInRoles AS ur ON r.RoleId = ur.RoleId ";
        inSql += "WHERE ur.UserId = " + WebSecurity.GetUserId(Membership.GetUser().UserName);
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'Gruppe' ";
        oCmd.CommandText += "And Value in (" + inSql + ") Order By Value";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          //var statuslistAll = new SelectListItem();
          //statuslistAll.Text = "Alle Mitglieder";
          //statuslistAll.Value = "100";
          //r.Add(statuslistAll);
          //var admin = new SelectListItem();
          //admin.Text = "Administrator";
          //admin.Value = "100";
          //r.Add(admin);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstAllGroups()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'Gruppe' ";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          var statuslistAll = new SelectListItem();
          statuslistAll.Text = "Alle Mitglieder";
          statuslistAll.Value = "10000";
          r.Add(statuslistAll);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstAgency()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select AgencyId, AgencyName From Agencies Order By AgencyName";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstDocType()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'DocType' Order By Value";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstEventSize()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'EventSize' Order By id";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var sizelistDefault = new SelectListItem();
          sizelistDefault.Text = "Select";
          sizelistDefault.Value = "";
          r.Add(sizelistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var sizelist = new SelectListItem();
            sizelist.Value = reader[0].ToString();
            sizelist.Text = reader[1].ToString();
            r.Add(sizelist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstEventType()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'EventType' Order By Value";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstDocStatus()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'DocStatus'";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstEventLocation()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select id, Value From ParamCollection Where Type = 'EventLocation'";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection = new SqlConnection(
          WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[0].ToString();
            statuslist.Text = reader[1].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstEvents()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select EvTitle, EventId From Events Order By EvTitle";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[1].ToString();
            statuslist.Text = reader[0].ToString();
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    private List<SelectListItem> lstEventsForDocs()
    {
      var r = new List<SelectListItem>();
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select EvTitle, EvStartDate, EventId From Events Order By EvStartDate";
        //oCmd.CommandText += "Where EvStartDate > getdate() Order By EvStartDate";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          var statuslistDefault = new SelectListItem();
          statuslistDefault.Text = "Select";
          statuslistDefault.Value = "";
          r.Add(statuslistDefault);
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            var statuslist = new SelectListItem();
            statuslist.Value = reader[2].ToString();
            statuslist.Text = reader[0] + " - " + reader[1];
            r.Add(statuslist);
          }
          oCmd.Connection.Close();
        }
        catch
        {
        }
        return r;
      }
    }

    public List<Documents> LstDocsOfEvent(int eventId)
    {
      var db = new KultiDocumentEntities();
      var oList = new List<Documents>();
      int docId;
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "SELECT DocId FROM Documents WHERE EventId = " + eventId;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            docId = Convert.ToInt16(reader[0].ToString());
            Documents oDoc = db.Documents.Single(d => d.DocId == docId);
            oList.Add((oDoc));
            oDoc = null;
          }
          oCmd.Connection.Close();
        }
        catch
        {
          return oList;
        }
      }
      return oList;
    }

    public List<Tuple<string, int, bool>> GetLocations(int eventId)
    {
      var TEventLocations = new List<Tuple<string, int, bool>>();

      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "getLocationsOfEvent";
        oCmd.CommandType = CommandType.StoredProcedure;
        oCmd.Parameters.Add(new SqlParameter("@eventId", eventId));
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            TEventLocations.Add(Tuple.Create(reader[0].ToString(), (int) reader[1], (int) reader[2] > 0));
          }
          oCmd.Connection.Close();
        }
        catch (Exception ex)
        {
          return TEventLocations;
        }
      }

      return TEventLocations;
    }

    //public List<string> GetEventAssignedLocations(int eventId)
    //{
    //  var l = new List<string>();

    //  using (var oCmd = new SqlCommand())
    //  {
    //    oCmd.CommandText = "getLocationsOfEvent";
    //    oCmd.CommandType = CommandType.StoredProcedure;
    //    oCmd.Parameters.Add(new SqlParameter("@eventId", eventId));
    //    oCmd.Connection =
    //      new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
    //    try
    //    {
    //      oCmd.Connection.Open();
    //      SqlDataReader reader = oCmd.ExecuteReader();
    //      while (reader.Read())
    //      {

    //        //TEventLocations.Add(Tuple.Create(reader[0].ToString(), (int)reader[1], (int)reader[2] > 0));
    //      }
    //      oCmd.Connection.Close();
    //    }
    //    catch (Exception ex)
    //    {
    //      return TEventLocations;
    //    }
    //  }

    //}

    public List<BlacklistViewModel> GetBlacklist()
    {
      var getBlacklist = new List<BlacklistViewModel>();
      try
      {
        var db = new BlacklistEntities();
        foreach (Blacklist blacklist in db.Blacklist.Where(x => x.ToDate > DateTime.Now).ToList())
        {
          var bl = new BlacklistViewModel();
          bl.CreatedBy = GetUserName(blacklist.UserID);
          bl.DateFrom = blacklist.FromDate;
          bl.DateTo = blacklist.ToDate;
          bl.FirstName = blacklist.FirstName;
          bl.LastName = blacklist.LastName;
          bl.Reason = blacklist.Reason;
          bl.Pk = blacklist.BlacklistID;

          getBlacklist.Add(bl);
        }
        return getBlacklist;
      }
      catch (Exception)
      {
        return getBlacklist;
      }
    }

    public List<Tuple<string, string, string, string>> GetBlogs()
    {
      var getBlogs = new List<Tuple<string, string, string, string>>();

      string sSQL = "SELECT Blog.Date, Blog.Title, Blog.BlogText, UserProfile.UserName ";
      sSQL += "FROM Blog INNER JOIN ";
      sSQL += "UserProfile ON Blog.UserId = UserProfile.UserId ORDER BY Blog.Date DESC";

      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = sSQL;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            //string fromTo = getBlogs..Value.ToShortDateString() + '-' + getBlogs.ToDate.Value.ToShortDateString();
            //string fullName = getBlogs.FirstName + ' ' + getBlogs.LastName;
            getBlogs.Add(Tuple.Create(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(),
                                      reader[3].ToString()));
          }
          oCmd.Connection.Close();
        }
        catch
        {
          return getBlogs;
        }
        return getBlogs;
      }
    }

  }
}