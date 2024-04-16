using Application.Data.Common;
using Common.Structures;

namespace Application.Services.Compile;

public interface ICompileService: IDisposable
{
    Task<CompileResult> SendSourceCode(string sourceId, string code);
}