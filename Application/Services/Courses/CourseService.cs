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
    
    public async Task<Course> CreateCourseAsync(Course course)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        course.Author = await context.Lecturers.FirstAsync(x => x.Id == course.AuthorId);
        var entry = await context.Courses.AddAsync(course);
        await context.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<Course> GetCourseAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var result = await context.Courses
            .Include(c =>c.Author)
            .Include(c =>c.Chapters)
                .ThenInclude(c=> c.Exercise)
            .FirstAsync(c => c.Id == id);
        return result;
    }

    public async Task UpdateCourseAsync(Course c)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var course = await context.Courses.FirstAsync(x=> x.Id == c.Id);
        course.Title = c.Title;
        course.Desc = c.Desc;
        context.Courses.Update(course);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCourse(Course course)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Courses.Remove(course);
        await context.SaveChangesAsync();
    }

    public async Task AddChapterToCourse(Course c, Chapter chapterModel)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        chapterModel.Body = "";
        var course = await context.Courses.FirstAsync(x=> x.Id == c.Id);
        chapterModel.Course = course;
        var chapter = (await context.Chapters.AddAsync(chapterModel)).Entity;
        
        await context.SaveChangesAsync();
        
        chapterModel.Course.Chapters.Add(chapter);
        if (chapter.IsExercise)
        {
            var exercise = (await context.Exercises.AddAsync(new Exercise(chapter))).Entity;
        }
        
        _logger.LogInformation($"Add chapter [{chapter.Id}] {chapter.Title} to course [{course.Id}] {course.Title} with exercise = {chapter.IsExercise}");
        await context.SaveChangesAsync();
    }

    public async Task UpdateChapterAsync(Chapter c)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var chapter = await context.Chapters.FirstAsync(x => x.Id == c.Id);
        chapter.Body = c.Body;
        context.Update(chapter);
        await context.SaveChangesAsync();
    }

    public async Task<Exercise> GetExerciseAsync(Chapter c)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Exercises
            .Include(e=>e.Implementations)
            .ThenInclude(i=>i.Language)
            .FirstAsync(e => e.ChapterId == c.Id);
    }
}