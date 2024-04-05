using System.Text.Json.Serialization;

namespace Common.Structures;

[Serializable]
public record CompileRequest
{
    public Guid ServiceId { get; }
    public string SourceId { get; }
    public string Code { get; }
    
    [JsonConstructor]
    public CompileRequest(Guid serviceId, string sourceId, string code)
    {
        ServiceId = serviceId;
        SourceId = sourceId;
        Code = code;
    }
}