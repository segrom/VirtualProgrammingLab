using Application.Data.Account;
using Application.Data.Common;
using Application.Data.Courses;
using Common.Structures;

namespace Application.Services.Courses;

public interface IExerciseService
{
    Task<Language[]> GetAllLanguagesAsync();
    Task AddImplWithLanguageAsync(int exerciseId, int languageId);
    Task UpdateImplAsync(Impl impl);
    Task<CompileRequest> NewDebugCompileRequest(Impl impl, User lecturerUser, string debugProgramCode);
    Task UpdateCompileRequest(QueueCompileResult result);
}