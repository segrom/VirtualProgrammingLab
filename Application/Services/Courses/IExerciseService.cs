using Application.Data.Common;
using Application.Data.Courses;

namespace Application.Services.Courses;

public interface IExerciseService
{
    Task<Language[]> GetAllLanguagesAsync();
    Task AddImplWithLanguageAsync(int exerciseId, int languageId);
    Task UpdateImplAsync(Impl impl);
}