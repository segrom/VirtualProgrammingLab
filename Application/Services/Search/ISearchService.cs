using Common.Students;

namespace Application.Services.Search;

public interface ISearchService
{
    Task<List<StudentGroup>> GetFirstGroupsAsync(int count, CancellationToken token = default);
    Task<List<StudentGroup>> SearchGroupsAsync(string query, CancellationToken token = default);
    Task<List<Student>> GetFirstStudentsAsync(int count, CancellationToken token = default);
    Task<List<Student>> SearchStudentsAsync(string query, CancellationToken token = default);
}