using Application.Data.Common;
using Common.Structures;

namespace Application.Services.Compile;

public interface ICompileService: IDisposable
{
    Task<QueueCompileResult> QueueCompileRequest(CompileRequest compileRequest);
}