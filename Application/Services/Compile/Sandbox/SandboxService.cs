using Application.Services.Courses;
using Common;
using Common.Account;
using Common.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Compile.Sandbox;

public class SandboxService: ISandboxService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private ILogger<SandboxService> _logger;

    public SandboxService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<SandboxService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<List<SandboxState>> GetStatesForUser(User user)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var states = await context.SandboxStates
            .Include(s=>s.Language)
            .Include(s=>s.CompileRequests)
            .Where(s => s.UserId == user.Id)
            .ToListAsync();

        return states
            .OrderByDescending(s => s.CompileRequests?.Max(r => r.FinishTime) ?? default)
            .ToList();
    }

    public async Task<SandboxState> CreateState(string userId, string name, int languageId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstAsync(u => u.Id.Equals(userId));
        var language = await context.Languages.FirstAsync(l => l.Id == languageId);
        var state = await context.SandboxStates.AddAsync(new SandboxState()
        {
            Code = language?.SandboxTemplateCode ?? "",
            Language = language,
            User = user,
            Name = name,
        });

        await context.SaveChangesAsync();

        return state.Entity;
    }

    public async Task DeleteState(int stateId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var state = await context.SandboxStates.FirstAsync(s => s.Id == stateId);
        context.SandboxStates.Remove(state);
        await context.SaveChangesAsync();
    }

    public async Task<(CompileRequest request, SandboxState state)> NewCompileRequest(string userId, int stateId, string code)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var state = await context.SandboxStates
            .Include(sandboxState => sandboxState.Language)
            .FirstAsync(s => s.Id == stateId);
        var user = await context.Users.FirstAsync(u => u.Id.Equals(userId));
        
        var request = await context.CompileRequests.AddAsync(
            new CompileRequest
            {
                Code = code,
                Language = state.Language,
                User = user,
                Status = CompileRequestStatus.Queued,
                Tests = "[no tests]",
                IsExercise = false,
                SandboxState = state,
                CreationTime = DateTimeOffset.Now
            });
        state.CompileRequests.Add(request.Entity);
        await context.SaveChangesAsync();
        return (request.Entity, state);
    }

    public async Task UpdateState(int stateId, string code)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var state = await context.SandboxStates.FirstAsync(s => s.Id == stateId);
        state.Code = code;
        context.Update(state);
        await context.SaveChangesAsync();
    }
}