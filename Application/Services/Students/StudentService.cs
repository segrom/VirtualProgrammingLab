using Application.Data;
using Application.Data.Courses;
using Application.Data.Students;
using Application.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Students;

public class StudentService: IStudentService
{
    private IUserService _userService;
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private ILogger<StudentService> _logger;

    public StudentService(IUserService userService, ILogger<StudentService> logger, 
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _userService = userService;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Student?> GetCurrentStudentAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await _userService.GetCurrentUser();
        if (user is null) return null;
        return await dbContext.Students
            .Include(l => l.User)
            .FirstOrDefaultAsync(s=>s.UserId == user.Id);
    }

    public async Task<List<Course>> GetStudentCoursesAsync(Student student)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var studentGroupId = student.GroupId;
        return await dbContext.Courses.Where(c => 
                c.Groups.Any(g => g.Id == studentGroupId) 
                && c.Status == CourseStatus.Published)
            .Include(c=>c.Chapters)
                .ThenInclude(c=>c.StudentStates)
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