using Common.Account;
using Common.Common;

namespace Application.Services.Compile.Sandbox;

public interface ISandboxService
{
    Task<List<SandboxState>> GetStatesForUser(User user);
    Task<SandboxState> CreateState(string userId, string name, int languageId);
    Task DeleteState(int stateId);
    Task<(CompileRequest request, SandboxState state)> NewCompileRequest(string userId, int stateId, string code);
    Task UpdateState(int stateId, string code);
}