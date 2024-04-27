using System.Text.RegularExpressions;
using Application.Services.Students;
using Application.Services.Users;
using Common;
using Common.Courses;
using Common.Lecturers;
using Common.Students;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Lecturers;

public class LecturerService : ILecturerService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private IUserService _userService;
    private ILogger<LecturerService> _logger;

    public LecturerService(ILogger<LecturerService> logger, 
        IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserService userService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _userService = userService;
    }

    public async Task<Lecturer?> GetCurrentLecturerAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await _userService.GetCurrentUser();
        if (user is null) return null;
        return await dbContext.Lecturers
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.UserId == user.Id);
    }
    
    public async Task<List<Course>> GetLecturerCoursesAsync(Lecturer lecturer)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Courses.Where(c => 
                c.AuthorId == lecturer.Id && c.Status != CourseStatus.Deleted)
            .Include(c=>c.Author)
            .ToListAsync();
    }
    
    public async Task<List<Course>> GetLecturerCoursesIncludeGroupsAsync(Lecturer lecturer)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Courses.Where(c => c.Status == CourseStatus.Published)
            .Include(c=>c.Author)
            .Include(c=>c.GroupCourseAssignments)
                .ThenInclude(a=>a.Group)
                    .ThenInclude(g=>g.Students)
                        .ThenInclude(s=>s.User)
            .Include(c=>c.GroupCourseAssignments)
                .ThenInclude(a=>a.Assigner)
            .ToListAsync();
    }

    public async Task<GroupCourseAssignment> AddCourseToGroup(Course c, StudentGroup g, Lecturer l)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var course = await dbContext.Courses.FirstAsync(x=>x.Id == c.Id);
        var group = await dbContext.StudentGroups.FirstAsync(x=>x.Id == g.Id);
        var lecturer = await dbContext.Lecturers
            .Include(x=>x.User)
            .FirstAsync(x=>x.Id == l.Id);

        var assignment =
            await dbContext.Assignments.FirstOrDefaultAsync(a => a.GroupId == group.Id && a.CourseId == course.Id);
        if (assignment is null)
        {
            
            assignment = new GroupCourseAssignment()
            {
                Group = group,
                Course = course,
                CreationDate = DateTimeOffset.Now
            };
            await dbContext.Assignments.AddAsync(assignment);
        }
        assignment.Assigner = lecturer.User;
        assignment.ChangeDate = DateTimeOffset.Now;
        
        await dbContext.SaveChangesAsync();
        return assignment;
    }

    public async Task DeleteChapterAsync(Chapter c)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var chapter = await dbContext.Chapters.FirstAsync(x=>x.Id == c.Id);
        dbContext.Chapters.Remove(chapter);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<StudentGroup>> GetAllGroups()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.StudentGroups.ToListAsync();
    }

    public async Task RemoveCourseFromGroup(Course c, StudentGroup g)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var assignments = await dbContext.Assignments.Where(x => x.CourseId == c.Id && x.GroupId == g.Id).ToListAsync();
        
        dbContext.Assignments.RemoveRange(assignments);
        await dbContext.SaveChangesAsync();
    }
}