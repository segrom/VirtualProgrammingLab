using Application.Services.Admin;
using Common;
using Common.Students;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Search;

public class SearchService: ISearchService
{
    
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private ILogger<SearchService> _logger;

    public SearchService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<SearchService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _logger.LogInformation("Created SearchService");
    }
    
    public async Task<List<StudentGroup>> GetFirstGroupsAsync(int count, CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        _logger.LogInformation("GetFirstGroupsAsync");
        return await dbContext.StudentGroups.OrderBy(g=>g.Id).Take(count)
            .Include(g=>g.Students).ToListAsync(cancellationToken: token);
    }
    
    public async Task<List<StudentGroup>> SearchGroupsAsync(string query, CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        _logger.LogInformation("SearchGroupsAsync");
        return await dbContext.StudentGroups.Where(s => s.Name.Contains(query)).ToListAsync(cancellationToken: token);
    }
    
    public async Task<List<Student>> GetFirstStudentsAsync(int count, CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        _logger.LogInformation("GetFirstStudentsAsync");
        return await dbContext.Students.OrderByDescending(s=>s.Id).Take(count).ToListAsync(cancellationToken: token);
    }
    
    public async Task<List<Student>> SearchStudentsAsync(string query, CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        _logger.LogInformation("SearchStudentsAsync");
        return await dbContext.Students.Where(s =>
            s.User.Name.Contains(query) 
            || s.User.Surname.Contains(query) 
            || s.User.Patronymic.Contains(query) 
            || s.Group.Name.Contains(query)).ToListAsync(cancellationToken: token);
    }
}