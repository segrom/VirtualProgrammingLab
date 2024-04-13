using Application.Data;
using Application.Data.Students;
using Application.Services.Admin;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Search;

public class SearchService: ISearchService
{
    
    private ApplicationDbContext _dbContext;
    private ILogger<SearchService> _logger;

    public SearchService(ApplicationDbContext dbContext, ILogger<SearchService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogInformation("Created SearchService");
    }
    
    public async Task<List<StudentGroup>> GetFirstGroupsAsync(int count, CancellationToken token = default)
    {
        _logger.LogInformation("GetFirstGroupsAsync");
        return await _dbContext.StudentGroups.OrderBy(g=>g.Id).Take(count)
            .Include(g=>g.Students).ToListAsync(cancellationToken: token);
    }
    
    public async Task<List<StudentGroup>> SearchGroupsAsync(string query, CancellationToken token = default)
    {
        _logger.LogInformation("SearchGroupsAsync");
        return await _dbContext.StudentGroups.Where(s => s.Name.Contains(query)).ToListAsync(cancellationToken: token);
    }
    
    public async Task<List<Student>> GetFirstStudentsAsync(int count, CancellationToken token = default)
    {
        _logger.LogInformation("GetFirstStudentsAsync");
        return await _dbContext.Students.OrderByDescending(s=>s.Id).Take(count).ToListAsync(cancellationToken: token);
    }
    
    public async Task<List<Student>> SearchStudentsAsync(string query, CancellationToken token = default)
    {
        _logger.LogInformation("SearchStudentsAsync");
        return await _dbContext.Students.Where(s =>
            s.User.Name.Contains(query) 
            || s.User.Surname.Contains(query) 
            || s.User.Patronymic.Contains(query) 
            || s.Group.Name.Contains(query)).ToListAsync(cancellationToken: token);
    }
}