using Application.Data;
using Application.Data.Account;
using Application.Data.Lecturers;
using Application.Data.Students;
using Application.Pages.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Admin;

public class AdminService: IAdminService
{
    private ApplicationDbContext _dbContext;
    private UserManager<User> _userManager;
    private ILogger<AdminService> _logger;

    public AdminService(ApplicationDbContext dbContext, ILogger<AdminService> logger, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _logger.LogInformation("Created AdminService");
    }

    public async Task<StudentGroup[]?> GetAllStudentGroupsAsync()
    {
        _logger.LogInformation("GetAllStudentGroupsAsync");
        return await _dbContext.StudentGroups
            .Include(g=>g.Students)
            .ToArrayAsync();
    }

    public async Task<bool> DeleteGroupAsync(StudentGroup group)
    {
        if (!_dbContext.StudentGroups.Contains(group)) return false;
        _dbContext.StudentGroups.Remove(group);
       var code = await _dbContext.SaveChangesAsync();
       return code == 1;
    }

    public async Task CreateOrUpdateGroupAsync(StudentGroup group)
    {
        if (_dbContext.StudentGroups.Contains(group))
        {
            _dbContext.StudentGroups.Update(group);
            _logger.LogInformation($"Update group [{group.Id}] {group.Name} students: {group.Students.Count}");
        }
        else
        {
            await _dbContext.StudentGroups.AddAsync(group);
            _logger.LogInformation($"Create group [{group.Id}] {group.Name} students: {group.Students.Count}");
        }
        await _dbContext.SaveChangesAsync();
       
    }
    
    public async Task<IdentityResult> CreateLecturerWithUserAsync(AdminLecturerModal.LecturerModel model)
    {
        var (res, user) = await CreatUserAsync(model.User, model.Password);
        if (!res.Succeeded) return res;
        
        await _userManager.AddToRoleAsync(user, RoleController.LecturerRole);
        
        model.Lecturer = (await _dbContext.Lecturers.AddAsync(model.Lecturer)).Entity;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created new lecturer {0} {1} {2} {3}", 
            model.Lecturer.Id, model.Lecturer.Faculty, model.User.UserName, model.User.GetFullName());
        return res;
    }

    public async Task<IdentityResult> UpdateLecturerWithUserAsync(AdminLecturerModal.LecturerModel model)
    {
        await _userManager.UpdateAsync(model.User);
        _dbContext.Lecturers.Update(model.Lecturer);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Lecturer {0} updated", model.Lecturer.Id);
        return IdentityResult.Success;
    }
    
    public async Task<IdentityResult> CreateStudentWithUserAsync(AdminStudentModal.StudentModel model)
    {
        var (res, user) = await CreatUserAsync(model.User, model.Password);
        if (!res.Succeeded) return res;
        
        await _userManager.AddToRoleAsync(user, RoleController.StudentRole);

        model.Student.GroupId = model.Student.Group.Id;
        model.Student = (await _dbContext.Students.AddAsync(model.Student)).Entity;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created new student {0} [{1}] {2}", 
            model.Student.Id, model.Student.Group.Name, model.Student.User.GetFullName() );
        return res;
    }

    public async Task<IdentityResult> UpdateStudentWithUserAsync(AdminStudentModal.StudentModel model)
    {
        await _userManager.UpdateAsync(model.User);
        _dbContext.Students.Update(model.Student);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Lecturer {0} updated", model.Student.Id);
        return IdentityResult.Success;
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