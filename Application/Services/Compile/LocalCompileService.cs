using Common.Common;
using Common.QueueStructures;

namespace Application.Services.Compile;

public class LocalCompileService: ICompileService
{
    public void Dispose()
    {
        
    }

    public async Task<QueueCompileResult> QueueCompileRequest(CompileRequest compileRequest)
    {
        await Task.Delay(3000);
        return new QueueCompileResult(Guid.Empty, compileRequest.Id, "Test complete result", "", DateTimeOffset.Now,
            TimeSpan.FromMilliseconds(10));
    }
}