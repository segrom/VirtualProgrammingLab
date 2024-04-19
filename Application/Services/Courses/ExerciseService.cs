using Application.Data;
using Application.Data.Account;
using Application.Data.Common;
using Application.Data.Courses;
using Application.Data.Students;
using Common.Structures;
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

    public async Task<(CompileRequest, ExerciseState?)> UpdateCompileRequest(QueueCompileResult result)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var request = await context.CompileRequests
            .Include(r=>r.ExerciseState)
            .FirstAsync(l=>l.Id == result.CompileRequestId);
        
        request.Status = string.IsNullOrEmpty(result.ResultErrors)
            ? CompileRequestStatus.Finished
            : CompileRequestStatus.FinishedWithErrors;
        request.Output = result.ResultOutput;
        request.Errors = result.ResultErrors;
        request.FinishTime = result.FinishTime;
        request.FinishTime = result.FinishTime;
        request.Duration = result.Duration;
        
        if (request.ExerciseState != null && request.ExerciseState.Status != ExerciseStatus.Completed)
        {
            request.ExerciseState.Status = request.Status == CompileRequestStatus.Finished
                ? ExerciseStatus.Completed
                : ExerciseStatus.Failed;
        }
        
        context.Update(request);
        await context.SaveChangesAsync();
        return (request, request.ExerciseState);
    }

    public async Task DeleteImplAsync(Impl i)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var impl = await context.Impls.FirstAsync(x=>x.Id == i.Id);
        context.Impls.Remove(impl);
        await context.SaveChangesAsync();
    }

    public async Task<CompileRequest> NewCompileRequest(Impl i, ExerciseState? st, Student s, string? code)
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
        return request.Entity;
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