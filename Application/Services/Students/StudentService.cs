using System.Security.Claims;
using Application.Data;
using Application.Data.Account;
using Application.Data.Courses;
using Application.Data.Students;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Students;

public class StudentService: IStudentService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<User> _userManager;
    private ApplicationDbContext _dbContext;
    private ILogger<StudentService> _logger;

    public StudentService(AuthenticationStateProvider authenticationStateProvider, 
        UserManager<User> userManager, ILogger<StudentService> logger, 
        ApplicationDbContext dbContext)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task<Student?> GetCurrentStudentAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(authState.User);
        return user.Student;
    }

    public async Task<List<Course>> GetStudentCoursesAsync(Student student)
    {
        var studentGroupId = student.GroupId;
        return await _dbContext.Courses.Where(c => 
                c.Groups.Any(g => g.Id == studentGroupId) 
                && c.Status == CourseStatus.Published)
            .Include(c=>c.Chapters)
            .Include(c=>c.Author)
            .ToListAsync();
    }

    public float GetStudentCourseProgressAsync(Student student, Course course)
    {
        var chaptersStatesCount =
            course.Chapters.Count(c => c.StudentStates.Any(s => s.StudentId == student.Id));
        return chaptersStatesCount / (float) course.Chapters.Count;
    }
}