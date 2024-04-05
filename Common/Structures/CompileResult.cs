using System.Text.Json.Serialization;

namespace Common.Structures;

[Serializable]
public record CompileResult
{
    public Guid ServiceId { get; }
    public string SourceId { get; }
    public string ResultOutput { get; }
    public string ResultErrors { get; }

    [JsonConstructor]
    public CompileResult(Guid serviceId, string sourceId, string resultOutput, string resultErrors)
    {
        ServiceId = serviceId;
        SourceId = sourceId;
        ResultOutput = resultOutput;
        ResultErrors = resultErrors;
    }
}