using System.Text.Json.Serialization;

namespace Common.QueueStructures;

[Serializable]
public record QueueCompileResult
{
    public Guid ServiceId { get; }
    public int CompileRequestId { get; }
    public string ResultOutput { get; }
    public string ResultErrors { get; }
    public DateTimeOffset FinishTime { get; }
    public TimeSpan Duration { get; }

    [JsonConstructor]
    public QueueCompileResult(Guid serviceId, int compileRequestId, string resultOutput, string resultErrors, DateTimeOffset finishTime, TimeSpan duration)
    {
        ServiceId = serviceId;
        CompileRequestId = compileRequestId;
        ResultOutput = resultOutput;
        ResultErrors = resultErrors;
        FinishTime = finishTime;
        Duration = duration;
    }
}