using Common;
using Common.Account;
using Common.Common;
using Common.Courses;
using Common.QueueStructures;
using Common.Students;
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
            TemplateCode = language.DefaultTemplateCode ?? "",
            TestsCode = language.DefaultTestsCode ?? "",
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
        impl.TemplateCode = i.TemplateCode;
        impl.TestsCode = i.TestsCode;
        context.Update(impl);
        await context.SaveChangesAsync();
    }

    public async Task<CompileRequest> NewDebugCompileRequest(Impl impl, User lecturerUser, string debugProgramCode)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var language = await context.Languages.FirstAsync(l=>l.Id == impl.LanguageId);
        var user = await context.Users.FirstAsync(e=>e.Id == lecturerUser.Id);

        var request = await context.CompileRequests.AddAsync(
            new CompileRequest
            {
                Code = debugProgramCode,
                Language = language,
                User = user,
                Status = CompileRequestStatus.Queued,
                Tests = impl.TestsCode,
            });
        await context.SaveChangesAsync();
        return request.Entity;
    }

    public async Task DeleteImplAsync(Impl i)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var impl = await context.Impls.FirstAsync(x=>x.Id == i.Id);
        context.Impls.Remove(impl);
        await context.SaveChangesAsync();
    }

    public async Task<(CompileRequest, ExerciseState)> NewCompileRequest(Impl i, ExerciseState? st, Student s, string? code)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var impl = await context.Impls
            .Include(x=>x.Language)
            .Include(x=>x.Exercise)
            .FirstAsync(x=>x.Id == i.Id);
        var student = await context.Students
            .Include(x=>x.User)
            .FirstAsync(e=>e.Id == s.Id);

        ExerciseState? state = null;
        if(st != null)
        {
            state = await context.ExerciseStates
                .FirstOrDefaultAsync(x => x.Id == st.Id);
        }
        
        state ??= await CreateNewExerciseStateAsync(context, impl, student);

        var request = await context.CompileRequests.AddAsync(
            new CompileRequest
            {
                Code = code,
                Language = impl.Language,
                User = student.User,
                Status = CompileRequestStatus.Queued,
                Tests = impl.TestsCode,
                ExerciseState = state,
                CreationTime = DateTimeOffset.Now
            });
        state.CompileRequests.Add(request.Entity);
        await context.SaveChangesAsync();
        return (request.Entity, state);
    }

    private async Task<ExerciseState> CreateNewExerciseStateAsync(ApplicationDbContext context, Impl impl, Student student)
    {
        var state = await context.ExerciseStates.AddAsync(new ExerciseState()
        {
            Exercise = impl.Exercise,
            Student = student,
            Status = ExerciseStatus.Started,
            Impl = impl
        });
        return state.Entity;
    }
}