using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.Configuration;
using System.Web.Mail;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Mvc4_Kulti.Filters;
using Mvc4_Kulti.Models;
using Mvc4_Kulti.ViewModels;
using WebMatrix.WebData;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Mvc4_Kulti.Controllers
{
  [Authorize]
  [InitializeSimpleMembership]
  public class AccountController : Controller
  {
    //
    //GET: /Account/ListUsers
    [Authorize(Roles = "Administrator")]
    public ActionResult ListUsers(ListAllUsersViewModel userViewModelModel)
    {
      var viewModel = new ListAllUsersViewModel
        {
          Users2 = getAllUsers()
        };
      return View(viewModel);
    }

    //
    //GET: /Account/ListUsersNoRoles
    [Authorize(Roles = "Administrator")]
    public ActionResult ListUsersNoRoles(ListAllUsersViewModel userViewModelModel)
    {
      var viewModel = new ListAllUsersViewModel
      {
        Users2 = getAllUsersNoRoles()
      };
      return View(viewModel);
    }

    //
    //GET: /Account/DeleteUser
    [Authorize(Roles = "Administrator")]
    public ActionResult DeleteUser(string userName)
    {
      string sql = "Select UserId From UserProfile Where UserName = '" + userName + "'";
      string sql2 = "Delete From webpages_UsersInRoles Where UserId = " + ExecSqlWithResult(sql);
      ExecuteSQL(sql2);
      sql2 = "Delete From UserProfile Where UserId = " + ExecSqlWithResult(sql);
      ExecuteSQL(sql2);
      return RedirectToAction("ListUsers", "Account");
    }

    //
    // GET: /Account/ManageUserRoles
    [Authorize(Roles = "Administrator")]
    public ActionResult ManageUserRoles(ManageMessageId? message)
    {
      var u = new ManageUserRolesViewModel();
      u.UserName = Request["UserName"];
      u.AllRoles = getAllRoles();
      u.UserHasRoles = getAllRolesForUser(u.UserName);
      u.UpdateResult = "";
      ViewBag.StatusMessage = message == ManageMessageId.ManageUserRolesSuccess
                                ? "Rollen wurden vergeben"
                                : "Fehler beim Rollen vergeben";
      return View(u);
    }

    //
    // POST: /Account/ManageUserRoles
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public ActionResult ManageUserRoles(string UserName, ManageUserRolesViewModel postedRoles)
    {
        string sql = string.Empty;
        var membership = (SimpleMembershipProvider) Membership.Provider;
        var roles = (SimpleRoleProvider) Roles.Provider;
        List<string> selectedRoles = postedRoles.AllRoles;
        sql = "Delete From webpages_UsersInRoles Where UserId = (Select UserId From UserProfile Where UserName = '";
        sql += UserName + "')";
        ExecuteSQL(sql);
        if ((selectedRoles != null) && (selectedRoles.Any()))
        {
            foreach (string Role in selectedRoles)
            {
                if (!roles.GetRolesForUser(UserName).Contains(Role))
                {
                    roles.AddUsersToRoles(new[] { UserName }, new[] { Role });
                }
            }
        }
        postedRoles.UpdateResult = "Rollen wurden vergeben";
        postedRoles.AllRoles = getAllRoles();
        postedRoles.UserHasRoles = getAllRolesForUser(UserName);
        return View(postedRoles);
    }

    //
    // GET: /Account/Login
    [AllowAnonymous]
    public ActionResult Login(string returnUrl)
    {
      ViewBag.ReturnUrl = returnUrl;
      return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Login(LoginModel model, string returnUrl)
    {
      //brun if  ( -- ModelState.IsValid -- && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
      if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
      {
        if (model.RememberMe)
        {
          FormsAuthentication.SetAuthCookie(model.UserName, true);          
        }

        return RedirectToLocal(returnUrl);
      }

      // If we got this far, something failed, redisplay form
      ModelState.AddModelError("", "The user name or password provided is incorrect.");
      return View(model);
    }

    //
    // POST: /Account/LogOff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult LogOff()
    {
      WebSecurity.Logout();
      return RedirectToAction("Index", "Home");
    }

    //
    // GET: /Account/Register
    [AllowAnonymous]
    public ActionResult Register()
    {
      if (!User.Identity.IsAuthenticated)
      { return (Redirect("https://www.spamhaus.org/rokso/")); }

     return View(); 
    }

    //
    // POST: /Account/Register
    [HttpPost]
    //[AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Register(RegisterModel model)
    {
      if (ModelState.IsValid)
      {
        // Attempt to register the user
        try
        {
          WebSecurity.CreateUserAndAccount(
            model.UserName,
            model.Password,
            new {model.Email}, false); //brun
          WebSecurity.Login(model.UserName, model.Password);
          return RedirectToAction("Index", "Home");
        }
        catch (MembershipCreateUserException e)
        {
          ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    //
    // POST: /Account/Disassociate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Disassociate(string provider, string providerUserId)
    {
      string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
      ManageMessageId? message = null;

      // Only disassociate the account if the currently logged in user is the owner
      if (ownerAccount == User.Identity.Name)
      {
        // Use a transaction to prevent the user from deleting their last login credential
        using (
          var scope = new TransactionScope(TransactionScopeOption.Required,
                                           new TransactionOptions {IsolationLevel = IsolationLevel.Serializable}))
        {
          bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
          if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
          {
            OAuthWebSecurity.DeleteAccount(provider, providerUserId);
            scope.Complete();
            message = ManageMessageId.RemoveLoginSuccess;
          }
        }
      }

      return RedirectToAction("Manage", new {Message = message});
    }

    //
    // GET: /Account/ManageEmail
    public ActionResult ManageEmail(ManageMessageId? message)
    {
      if (ViewBag.StatusMessage != null)
      {
        ViewBag.StatusMessage = message == ManageMessageId.ChangeEmailSuccess ? "Email wurde aktualisiert." : "Ein Fehler aufgetreten";
      }

      ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
      ViewBag.ReturnUrl = Url.Action("ManageEmail");
      ViewBag.OldEmail = getCurrentEmail(WebSecurity.GetUserId(User.Identity.Name));
      return View();
    }

    //
    // POST: /Account/ManageEmail
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ManageEmail(LocalEmailModel model)
    {
      bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
      ViewBag.HasLocalPassword = hasLocalAccount;

      bool changeEmailSucceeded = true;
      if (hasLocalAccount)
      {
        if (ModelState.IsValid)
        {
          // ChangePassword will throw an exception rather than return false in certain failure scenarios.
          try
          {
            //changeEmailSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
            UpdateEmail(model.NewEmail, WebSecurity.GetUserId(User.Identity.Name));
          }
          catch (Exception)
          {
            changeEmailSucceeded = false;
          }

          if (changeEmailSucceeded)
          {
            return RedirectToAction("ManageEmail", new {Message = ManageMessageId.ChangeEmailSuccess});
          }
          else
          {
            ModelState.AddModelError("", "The current email is incorrect or the new email is invalid.");
          }
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    //
    // GET: /Account/Manage
    public ActionResult Manage(ManageMessageId? message)
    {
      string[] rolesArray;
      ViewBag.StatusMessage =
        message == ManageMessageId.ChangePasswordSuccess
          ? "Dein Passwort wurde aktualisiert."
          : message == ManageMessageId.SetPasswordSuccess
              ? "Dein Passwort wurde gesetzt."
              : message == ManageMessageId.RemoveLoginSuccess
                  ? "The external login was removed."
                  : "";
      ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));

      RolePrincipal r = (RolePrincipal)User;
      rolesArray = r.GetRoles();
      ViewBag.Roles = rolesArray;

      ViewBag.ReturnUrl = Url.Action("Manage");
      return View();
    }

    //
    // POST: /Account/Manage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Manage(LocalPasswordModel model)
    {
      bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
      ViewBag.HasLocalPassword = hasLocalAccount;
      ViewBag.ReturnUrl = Url.Action("Manage");
      if (hasLocalAccount)
      {
        if (ModelState.IsValid)
        {
          // ChangePassword will throw an exception rather than return false in certain failure scenarios.
          bool changePasswordSucceeded;
          try
          {
            changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword,
                                                                 model.NewPassword);
          }
          catch (Exception)
          {
            changePasswordSucceeded = false;
          }

          if (changePasswordSucceeded)
          {
            return RedirectToAction("Manage", new {Message = ManageMessageId.ChangePasswordSuccess});
          }
          else
          {
            ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
          }
        }
      }
      else
      {
        // User does not have a local password so remove any validation errors caused by a missing
        // OldPassword field
        ModelState state = ModelState["OldPassword"];
        if (state != null)
        {
          state.Errors.Clear();
        }

        if (ModelState.IsValid)
        {
          try
          {
            WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
            return RedirectToAction("Manage", new {Message = ManageMessageId.SetPasswordSuccess});
          }
          catch (Exception e)
          {
            ModelState.AddModelError("", e);
          }
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    //
    // POST: /Account/ExternalLogin
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLogin(string provider, string returnUrl)
    {
      return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new {ReturnUrl = returnUrl}));
    }

    //
    // GET: /Account/ExternalLoginCallback
    [AllowAnonymous]
    public ActionResult ExternalLoginCallback(string returnUrl)
    {
      AuthenticationResult result =
        OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new {ReturnUrl = returnUrl}));
      if (!result.IsSuccessful)
      {
        return RedirectToAction("ExternalLoginFailure");
      }

      if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
      {
        return RedirectToLocal(returnUrl);
      }

      if (User.Identity.IsAuthenticated)
      {
        // If the current user is logged in add the new account
        OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
        return RedirectToLocal(returnUrl);
      }
      else
      {
        // User is new, ask for their desired membership name
        string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
        ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
        ViewBag.ReturnUrl = returnUrl;
        return View("ExternalLoginConfirmation",
                    new RegisterExternalLoginModel {UserName = result.UserName, ExternalLoginData = loginData});
      }
    }

    //
    // POST: /Account/ExternalLoginConfirmation
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
    {
      string provider = null;
      string providerUserId = null;

      if (User.Identity.IsAuthenticated ||
          !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
      {
        return RedirectToAction("Manage");
      }

      if (ModelState.IsValid)
      {
        // Insert a new user into the database
        using (var db = new UsersContext())
        {
          UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
          // Check if user already exists
          if (user == null)
          {
            // Insert name into the profile table
            db.UserProfiles.Add(new UserProfile {UserName = model.UserName});
            db.SaveChanges();

            OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
            OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

            return RedirectToLocal(returnUrl);
          }
          else
          {
            ModelState.AddModelError("UserName", "User-Name existiert bereits. Anderer User-Name wählen.");
          }
        }
      }

      ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
      ViewBag.ReturnUrl = returnUrl;
      return View(model);
    }

    //
    // GET: /Account/ExternalLoginFailure
    [AllowAnonymous]
    public ActionResult ExternalLoginFailure()
    {
      return View();
    }

    [AllowAnonymous]
    [ChildActionOnly]
    public ActionResult ExternalLoginsList(string returnUrl)
    {
      ViewBag.ReturnUrl = returnUrl;
      return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
    }

    [ChildActionOnly]
    public ActionResult RemoveExternalLogins()
    {
      ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
      var externalLogins = new List<ExternalLogin>();
      foreach (OAuthAccount account in accounts)
      {
        AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

        externalLogins.Add(new ExternalLogin
          {
            Provider = account.Provider,
            ProviderDisplayName = clientData.DisplayName,
            ProviderUserId = account.ProviderUserId,
          });
      }

      ViewBag.ShowRemoveButton = externalLogins.Count > 1 ||
                                 OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
      return PartialView("_RemoveExternalLoginsPartial", externalLogins);
    }

    //
    // GET: /Account/ResetPassword    
    [AllowAnonymous]
    public ActionResult ResetPasswort()
    {
      ViewBag.Confirm = "frm";
      return View();
    }

    //
    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public  ActionResult ResetPasswort(string username, string email)
    {
      try
      {
        var hplv = new HelpersViewModel();
        
        //check credentials
        int userid = hplv.GetUserId(username);
        string sEmail = getCurrentEmail(userid);

        //generate new password & send by email
        if (sEmail.ToLower() == email.ToLower())
        {
          string newpassword = Membership.GeneratePassword(8, 0); 
          string confirmationToken = WebSecurity.GeneratePasswordResetToken(username, 1440);
          WebSecurity.ResetPassword(confirmationToken, newpassword);
          MailMessage m = new MailMessage();
          m.BodyFormat = MailFormat.Html;
          m.From = "admin@kulturfabrik.ch";
          m.Subject = "Passwort erstellt";
          m.Body = "Hier das neue Passwort, du kannst es jederzeit ändern:<br/>" + newpassword;
          m.Body += "<br/>L.G. Kulti";
          m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
          m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "admin@kulturfabrik.ch");
          m.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "Bass5494");
          //To reset your password click on this link http://localhost/Account/ResetPasswordConfirmation/@ViewBag.ConfirmationToken
          m.To = email.ToLower();
          SmtpMail.SmtpServer = "localhost";
          SmtpMail.Send(m);
          ViewBag.Confirm = "ok";
          return View("ResetPasswort");
        }
        ViewBag.Confirm = "false";
        return View("ResetPasswort");
        }
      catch
      {
        ViewBag.Confirm = "false";
        return View("ResetPasswort");
      }
    }

    #region Helpers

    public enum ManageMessageId
    {
      ChangePasswordSuccess,
      SetPasswordSuccess,
      RemoveLoginSuccess,
      ChangeEmailSuccess,
      AddRoleSuccess,
      ManageUserRolesSuccess,
    }

    private ActionResult RedirectToLocal(string returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      else
      {
        return RedirectToAction("Index", "Home");
      }
    }

    private static string ErrorCodeToString(MembershipCreateStatus createStatus)
    {
      // See http://go.microsoft.com/fwlink/?LinkID=177550 for
      // a full list of status codes.
      switch (createStatus)
      {
        case MembershipCreateStatus.DuplicateUserName:
          return "User name already exists. Please enter a different user name.";

        case MembershipCreateStatus.DuplicateEmail:
          return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

        case MembershipCreateStatus.InvalidPassword:
          return "The password provided is invalid. Please enter a valid password value.";

        case MembershipCreateStatus.InvalidEmail:
          return "The e-mail address provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidAnswer:
          return "The password retrieval answer provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidQuestion:
          return "The password retrieval question provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidUserName:
          return "The user name provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.ProviderError:
          return
            "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

        case MembershipCreateStatus.UserRejected:
          return
            "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

        default:
          return
            "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
      }
    }

    private bool UpdateEmail(string email, int userid)
    {
      bool updateSuccess = false;
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Update UserProfile Set Email = '" + email + "' Where Userid = " + userid;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          oCmd.ExecuteNonQuery();
          updateSuccess = true;
          oCmd.Connection.Close();
        }
        catch
        {
        }
      }
      return updateSuccess;
    }

    private string getCurrentEmail(int userid)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select Email From UserProfile Where Userid = " + userid;
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
        catch (Exception ex)
        {
          return ex.Message;
        }
        return ("not implemented");
      }
    }

    private List<string> getAllRoles()
    {
      var lstRoles = new List<string>();
      var roles = (SimpleRoleProvider) Roles.Provider;
      foreach (string roleName in roles.GetAllRoles())
      {
        lstRoles.Add(roleName);
      }
      return lstRoles;
    }

    private List<string> getAllRolesForUser(string UserName)
    {
      var roles = (SimpleRoleProvider) Roles.Provider;
      return roles.GetRolesForUser(UserName).ToList();
    }

    private List<Tuple<string, string>> getAllUsers()
    {
      //var users = new List<string>();
      var users2 = new List<Tuple<string, string>>();

      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "Select UserName, Email From UserProfile Order by UserName";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            users2.Add(new Tuple<string, string>(reader[0].ToString(), reader[1].ToString()));
            //users.Add(reader[0].ToString());
          }
          oCmd.Connection.Close();
        }
        catch
        {
          //users.Add("Fehler!!");
          users2.Add(new Tuple<string, string>("Fehler!!", "Fehler!!"));
        }
      }
      return users2;
    }

    private List<Tuple<string, string>> getAllUsersNoRoles()
    {
      //var users = new List<string>();
      var users2 = new List<Tuple<string, string>>();

      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = "SELECT UserName, Email FROM UserProfile WHERE (UserId NOT IN (SELECT UserId FROM webpages_UsersInRoles)) ORDER BY UserName";
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          SqlDataReader reader = oCmd.ExecuteReader();
          while (reader.Read())
          {
            //users.Add(reader[0].ToString());
            users2.Add(new Tuple<string, string>(reader[0].ToString(), reader[1].ToString()));
          }
          oCmd.Connection.Close();
        }
        catch
        {
          //users.Add("Fehler!!");
          users2.Add(new Tuple<string, string>("Fehler!!", "Fehler!!"));
        }
      }
      return users2;
    }

    private void ExecuteSQL(string sql)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = sql;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection =
          new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        try
        {
          oCmd.Connection.Open();
          oCmd.ExecuteNonQuery();
          oCmd.Connection.Close();
        }
        catch
        {}
      }
    }

    private string ExecSqlWithResult(string sql)
    {
      using (var oCmd = new SqlCommand())
      {
        oCmd.CommandText = sql;
        oCmd.CommandType = CommandType.Text;
        oCmd.Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
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
          return "";
        }
      }
      return ("not implemented");
    }

    internal class ExternalLoginResult : ActionResult
    {
      public ExternalLoginResult(string provider, string returnUrl)
      {
        Provider = provider;
        ReturnUrl = returnUrl;
      }

      public string Provider { get; private set; }
      public string ReturnUrl { get; private set; }

      public override void ExecuteResult(ControllerContext context)
      {
        OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
      }
    }

    #endregion
  }
}