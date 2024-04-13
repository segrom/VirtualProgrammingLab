using Application.Data;
using Application.Data.Account;
using Application.Data.Courses;
using Application.Data.Lecturers;
using Application.Services.Students;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Lecturers;

public class LecturerService : ILecturerService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<User> _userManager;
    private ApplicationDbContext _dbContext;
    private ILogger<LecturerService> _logger;

    public LecturerService(AuthenticationStateProvider authenticationStateProvider, 
        UserManager<User> userManager, ILogger<LecturerService> logger, 
        ApplicationDbContext dbContext)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Lecturer?> GetCurrentLecturerAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(authState.User);
        return user.Lecturer;
    }
    
    public async Task<List<Course>> GetLecturerCoursesAsync(Lecturer lecturer)
    {
        return await _dbContext.Courses.Where(c => 
                c.AuthorId == lecturer.Id && c.Status != CourseStatus.Deleted)
            .Include(c=>c.Author)
            .ToListAsync();
    }
}