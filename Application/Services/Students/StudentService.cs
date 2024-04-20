using Application.Services.Users;
using Common;
using Common.Courses;
using Common.Students;
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
            .Include(s => s.User)
            .Include(s=> s.Group)
            .FirstOrDefaultAsync(s=>s.UserId == user.Id);
    }

    public async Task<List<Course>> GetStudentCoursesAsync(Student student)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Courses.Where(c => 
                c.Groups.Any(g => g.Id == student.GroupId) 
                && c.Status == CourseStatus.Published)
            .Include(c=>c.Chapters)
                .ThenInclude(c=>c.StudentStates)
            .Include(c=>c.Chapters)
                .ThenInclude(c=>c.Exercise)
                    .ThenInclude(c=>c.States.Where(s=>s.StudentId == student.Id))
            .Include(c=>c.Author)
                .ThenInclude(a=>a.User)
            .ToListAsync();
    }

    public float GetStudentCourseProgress(Student student, Course course)
    {
        var chaptersStatesCount =
            course.Chapters.Where(c => !c.IsExercise)
                .Count(c => c.StudentStates.Any(s => s.StudentId == student.Id));
        var exerciseStatesCount =
            course.Chapters.Where(c => c.IsExercise)
                .Count(c=>c.Exercise.States.Any(s=>
                    s.StudentId == student.Id 
                    && s.Status == ExerciseStatus.Completed));
        return (chaptersStatesCount + exerciseStatesCount) / (float) course.Chapters.Count;
    }

    public async Task<(Chapter, ChapterStudentState)> UpdateChapterState(Chapter c, Student s)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var chapter = await dbContext.Chapters
            .Include(x=>x.StudentStates)
            .Include(x=>x.Exercise)
                .ThenInclude(e=>e.Implementations)
                    .ThenInclude(i=>i.Language)
            .Include(x=>x.Exercise)
                .ThenInclude(e=>e.States.Where(x=>x.StudentId == s.Id))
                    .ThenInclude(x=>x.CompileRequests.Where(r=>r.User.Id == s.UserId))
            .FirstAsync(x => x.Id == c.Id);

        var state = chapter.StudentStates.FirstOrDefault(x => x.StudentId == s.Id);

        if (state is null)
        {
            var student = await dbContext.Students.FirstAsync(x => x.Id == s.Id);
            state = (await dbContext.ChapterStudentStates.AddAsync(new ChapterStudentState()
            {
                Student = student,
                Chapter = chapter,
                OpeningsCount = 1,
                FirstOpenTime = DateTimeOffset.Now,
                LastOpenTime = DateTimeOffset.Now,
            })).Entity;
            chapter.StudentStates.Add(state);
            await dbContext.SaveChangesAsync();
            return (chapter, state);
        }
        
        dbContext.Update(state);
        state.OpeningsCount++;
        state.LastOpenTime = DateTimeOffset.Now;
        await dbContext.SaveChangesAsync();
        return (chapter, state);
    }

    public async Task<List<ExerciseState>> GetExerciseStatesAsync(Exercise e, Student s)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ExerciseStates
            .Include(x => x.CompileRequests)
            .Include(x => x.Impl)
            .Where(x => x.ExerciseId == e.Id && x.StudentId == s.Id)
            .OrderByDescending(x=>x.CompileRequests.OrderByDescending(r=>r.CreationTime).First())
            .ToListAsync();
    }

    public async Task<Course> GetStudentCourseAsync(int courseId, Student s)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var result = await context.Courses
            .Include(c =>c.Author)
            .Include(c =>c.Chapters)
                .ThenInclude(c=> c.Exercise)
                    .ThenInclude(e=>e.Implementations)
                        .ThenInclude(i=>i.Language)
            .Include(c =>c.Chapters)
                .ThenInclude(c=>c.StudentStates.Where(x=>x.StudentId == x.Id))
            .Include(c=>c.Groups.Where(g=>g.Id == s.GroupId))
            .FirstAsync(c => c.Id == courseId);
        return result;
    }
}