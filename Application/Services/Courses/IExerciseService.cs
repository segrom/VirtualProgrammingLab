using Application.Data.Account;
using Application.Data.Common;
using Application.Data.Courses;
using Application.Data.Students;
using Common.Structures;

namespace Application.Services.Courses;

public interface IExerciseService
{
    Task<Language[]> GetAllLanguagesAsync();
    Task AddImplWithLanguageAsync(int exerciseId, int languageId);
    Task UpdateImplAsync(Impl impl);
    Task<CompileRequest> NewDebugCompileRequest(Impl impl, User lecturerUser, string debugProgramCode);
    Task<(CompileRequest, ExerciseState?)> UpdateCompileRequest(QueueCompileResult result);
    Task DeleteImplAsync(Impl impl);
    Task<CompileRequest> NewCompileRequest(Impl i, ExerciseState? st, Student s, string? code);
}