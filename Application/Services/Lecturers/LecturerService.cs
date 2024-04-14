using Application.Data;
using Application.Data.Account;
using Application.Data.Courses;
using Application.Data.Lecturers;
using Application.Services.Students;
using Application.Services.Users;
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
}