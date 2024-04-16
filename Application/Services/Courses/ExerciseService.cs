using Application.Data;
using Application.Data.Common;
using Application.Data.Courses;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Courses;

public class ExerciseService: IExerciseService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private ILogger<CourseService> _logger;

    public ExerciseService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<CourseService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Language[]> GetAllLanguagesAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Languages.ToArrayAsync();
    }

    // api styled endpoint
    public async Task AddImplWithLanguageAsync(int exerciseId, int languageId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var language = await context.Languages.FirstAsync(l=>l.Id == languageId);
        var exercise = await context.Exercises.Include(e=>e.Chapter).FirstAsync(e=>e.Id == exerciseId);

        var impl = new Impl()
        {
            PatternCode = "",
            TestsCode = "",
            Exercise = exercise,
            Language = language
        };

        impl = (await context.Impls.AddAsync(impl)).Entity;
        _logger.LogInformation($"Create new impl [{impl.Id}] for exercise [{exercise.Id}] {exercise.Chapter.Id} with language [{language.Id}] {language.Name}");
        await context.SaveChangesAsync();
    }

    public async Task UpdateImplAsync(Impl i)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var impl = await context.Impls.FirstAsync(x => x.Id == i.Id);
        impl.PatternCode = i.PatternCode;
        impl.TestsCode = i.TestsCode;
        context.Update(impl);
        await context.SaveChangesAsync();
    }
}