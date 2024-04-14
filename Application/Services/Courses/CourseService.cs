using Application.Data;
using Application.Data.Account;
using Application.Data.Courses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Courses;

public class CourseService: ICourseService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Course> CreateCourseAsync(Course model)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entry = await context.Courses.AddAsync(model);
        await context.SaveChangesAsync();
        return entry.Entity;
    }
}