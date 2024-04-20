using Common.Account;
using Common.Common;
using Common.Courses;
using Common.QueueStructures;
using Common.Students;

namespace Application.Services.Courses;

public interface IExerciseService
{
    Task<Language[]> GetAllLanguagesAsync();
    Task AddImplWithLanguageAsync(int exerciseId, int languageId);
    Task UpdateImplAsync(Impl impl);
    Task<CompileRequest> NewDebugCompileRequest(Impl impl, User lecturerUser, string debugProgramCode);
    Task DeleteImplAsync(Impl impl);
    Task<(CompileRequest, ExerciseState)> NewCompileRequest(Impl i, ExerciseState? st, Student s, string? code);
}