using Common.Structures;

namespace Application.Rabbit.Compile;

public interface ICompileService: IDisposable
{
    Task<CompileResult> SendSourceCode(string sourceId, string code);
}