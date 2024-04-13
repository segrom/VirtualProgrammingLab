using System.ComponentModel.DataAnnotations;
using Application.Pages.Admin;
using Microsoft.AspNetCore.Identity;

namespace Application.Data.Account;

public enum UserRole
{
    Admin, Lecturer, Student
}

public class UserFormModel
{
    
    public UserFormModel(Admin.UserWithRoles? value = null)
    {
        Origin = value;
        
        if (value is not { } user) return;
        
        Code = user.Base.UserName;
        
        foreach (string userRole in user.Roles)
        {
            if(!Enum.TryParse(userRole, out UserRole role)) continue;
            Role |= role;
        }
    }
    
    public Admin.UserWithRoles? Origin { get; set; }
    [Required]
    [StringLength(20, MinimumLength = 5 )]
    public string Code { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Patronymic { get; set; }
    [Required]
    public UserRole Role { get; set; }
    public string Password { get; set; }
}