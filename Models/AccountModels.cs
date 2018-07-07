using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Mvc4_Kulti.Models
{
  public class UsersContext : DbContext
  {
    public UsersContext()
      : base("DefaultConnection")
    {}

    public DbSet<UserProfile> UserProfiles { get; set; }
  }

  [Table("UserProfile")]
  public class UserProfile
  {
    [Key]
    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
  }

  public class RegisterExternalLoginModel
  {
    [Required]
    [Display(Name = "User name")]
    public string UserName { get; set; }

    public string ExternalLoginData { get; set; }
  }

  public class LocalPasswordModel
  {
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string OldPassword { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = " {0} muss mindestens {2} Zeichen besitzen.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Das neue Passwort")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Neues Passwort wiederholen")]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Neues Passwort ungleich Wiederholung")]
    public string ConfirmPassword { get; set; }
  }

  public class LocalEmailModel
  {
    [Required]
    [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Bitte gültiges Email angeben")]
    [Display(Name = "Neues Email")]
    public string NewEmail { get; set; }
  }

  public class ManageRoles
  {
    [Required]
    [Display(Name = "Neue Rolle")]
    public string NewRole { get; set; }
  }

  public class ResetPassword
  {
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; }
    [Display(Name = "Email")]
    public string Email { get; set; }
  }



  public class ManageUserRoles
  {
    [Required]
    [Display(Name = "User")]
    public int UserId { get; set; }

    [Display(Name = "Roles")]
    public List<string> Roles { get; set; }


  }

  public class LoginModel
  {
    [Required]
    [Display(Name = "User-Name")]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }

  public class RegisterModel
  {
    [Required]
    [Display(Name = "User name")]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

  }

  public class ExternalLogin
  {
    public string Provider { get; set; }
    public string ProviderDisplayName { get; set; }
    public string ProviderUserId { get; set; }
  }
}
