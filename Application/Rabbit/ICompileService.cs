using Common.Structures;

namespace Application.Rabbit;

public interface ICompileService: IDisposable
{
    Task<CompileResult> SendSourceCode(string sourceId, string code);
}