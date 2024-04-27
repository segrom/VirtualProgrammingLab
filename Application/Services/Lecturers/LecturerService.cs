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
            .Include(c=>c.Groups)
                .ThenInclude(g=>g.Students)
                    .ThenInclude(s=>s.User)
            .ToListAsync();
    }

    public async Task AddCourseToGroup(Course c, StudentGroup g)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var course = await dbContext.Courses.FirstAsync(x=>x.Id == c.Id);
        var group = await dbContext.StudentGroups.FirstAsync(x=>x.Id == g.Id);
        
        group.Courses.Add(course);
        
        dbContext.Update(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddCoursesToGroup(List<Course> c, StudentGroup g)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var ids = c.Select(c => c.Id);
        var courses = await dbContext.Courses.Where(x=> ids.Contains(x.Id)).ToListAsync();
        var group = await dbContext.StudentGroups.FirstAsync(x=>x.Id == g.Id);
        
        for (int i = 0; i < courses.Count; i++)
        {
            if (group.Courses.Contains(courses[i])) courses.Remove(courses[i]);
        }
        
        group.Courses.AddRange(courses);
        
        dbContext.Update(group);
        await dbContext.SaveChangesAsync();
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
        var course = await dbContext.Courses
            .Include(x=>x.Groups)
            .FirstAsync(x=>x.Id == c.Id);
        var group = await dbContext
            .StudentGroups
            .Include(x=>x.Courses)
            .FirstAsync(x=>x.Id == g.Id);
        
        dbContext.Update(group);
        
        group.Courses.Remove(course);
        
        await dbContext.SaveChangesAsync();
    }
}