using Common.Common;
using Common.QueueStructures;

namespace Application.Services.Compile;

public interface ICompileService: IDisposable
{
    Task<QueueCompileResult> QueueCompileRequest(CompileRequest compileRequest);
}