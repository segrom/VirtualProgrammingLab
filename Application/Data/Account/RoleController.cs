using Microsoft.AspNetCore.Identity;

namespace Application.Data.Account;

public class RoleController
{

    public const string AdminRole = "Admin";
    public const string LecturerRole = "Lecturer";
    public const string StudentRole = "Student";
    
    public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string adminUsername = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME") 
                               ?? throw new Exception("Env variable DEFAULT_ADMIN_NAME not found");
        string password = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") 
                          ?? throw new Exception("Env variable DEFAULT_ADMIN_PASSWORD not found");
        if (await roleManager.FindByNameAsync(AdminRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }
        if (await roleManager.FindByNameAsync(StudentRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(StudentRole));
        }
        if (await roleManager.FindByNameAsync(LecturerRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(LecturerRole));
        }
        if (await userManager.FindByNameAsync(adminUsername) == null)
        {
            IdentityUser admin = new IdentityUser { UserName = adminUsername };
            IdentityResult result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }
    }
}