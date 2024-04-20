using Application.Data;
using Application.Data.Account;
using Application.Data.Common;
using Application.Data.Students;
using Application.Pages.Admin;
using Application.Pages.Admin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Admin;

public class AdminService: IAdminService
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private UserManager<User> _userManager;
    private ILogger<AdminService> _logger;

    public AdminService(IDbContextFactory<ApplicationDbContext>  dbContextFactory, ILogger<AdminService> logger, UserManager<User> userManager)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _userManager = userManager;
        _logger.LogInformation("Created AdminService");
    }

    public async Task<StudentGroup[]?> GetAllStudentGroupsAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        _logger.LogInformation("GetAllStudentGroupsAsync");
        return await dbContext.StudentGroups
            .Include(g=>g.Students)
            .ToArrayAsync();
    }

    public async Task<bool> DeleteGroupAsync(StudentGroup group)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (!dbContext.StudentGroups.Contains(group)) return false;
        dbContext.StudentGroups.Remove(group);
       var code = await dbContext.SaveChangesAsync();
       return code == 1;
    }

    public async Task CreateOrUpdateGroupAsync(StudentGroup group)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (dbContext.StudentGroups.Contains(group))
        {
            dbContext.StudentGroups.Update(group);
            _logger.LogInformation($"Update group [{group.Id}] {group.Name} students: {group.Students.Count}");
        }
        else
        {
            await dbContext.StudentGroups.AddAsync(group);
            _logger.LogInformation($"Create group [{group.Id}] {group.Name} students: {group.Students.Count}");
        }
        await dbContext.SaveChangesAsync();
       
    }
    
    public async Task<IdentityResult> CreateLecturerWithUserAsync(LecturerModel model)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var (res, u) = await CreatUserAsync(model.User, model.Password);
        if (!res.Succeeded) return res;
        
        await _userManager.AddToRoleAsync(u, RoleController.LecturerRole);
        model.Lecturer.User = await dbContext.Users.FirstAsync(x => x.Id == u.Id);
        model.Lecturer = (await dbContext.Lecturers.AddAsync(model.Lecturer)).Entity;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Created new lecturer {0} {1} {2} {3}", 
            model.Lecturer.Id, model.Lecturer.Faculty, model.User.UserName, model.User.GetFullName());
        return res;
    }

    public async Task<IdentityResult> UpdateLecturerWithUserAsync(LecturerModel model)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await _userManager.UpdateAsync(model.User);
        dbContext.Lecturers.Update(model.Lecturer);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Lecturer {0} updated", model.Lecturer.Id);
        return IdentityResult.Success;
    }
    
    public async Task<IdentityResult> CreateStudentWithUserAsync(StudentModel model)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var (res, u) = await CreatUserAsync(model.User, model.Password);
        if (!res.Succeeded) return res;
        
        await _userManager.AddToRoleAsync(u, RoleController.StudentRole);
        
        model.Student.User = await dbContext.Users.FirstAsync(x => x.Id == u.Id);
        model.Student.Group = await dbContext.StudentGroups.FirstAsync(x => x.Id == model.Student.Group.Id);
        var result = (await dbContext.Students.AddAsync(model.Student)).Entity;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Created new student {0} [{1}] {2}", 
            result.Id, result.Group.Name, result.User.GetFullName() );
        return res;
    }

    public async Task<IdentityResult> UpdateStudentWithUserAsync(StudentModel model)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await _userManager.UpdateAsync(model.User);
        dbContext.Students.Update(model.Student);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Lecturer {0} updated", model.Student.Id);
        return IdentityResult.Success;
    }

    public async Task<List<CompileRequest>> GetUserCompileRequests(User u)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompileRequests.Where(c => c.UserId == u.Id).ToListAsync();
    }

    private async Task<(IdentityResult, User)> CreatUserAsync(User user, string passsword)
    {
        var result = await _userManager.CreateAsync(user,passsword);
        if (!result.Succeeded)
        {
            _logger.LogError("Error creation user {0}: {1}", user.UserName, result.Errors);
            return (result, user);
        }
        _logger.LogInformation("Created new user {0}", user.UserName);
        return (result, user);
    }
}