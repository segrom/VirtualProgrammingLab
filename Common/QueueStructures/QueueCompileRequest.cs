using System.Text.Json.Serialization;

namespace Common.QueueStructures;

[Serializable]
public record QueueCompileRequest
{
    public Guid ServiceId { get; }
    public int CompileRequestId { get; }
    public string Solution { get; set; }
    public string Tests { get; set; }
    public bool IsExercise { get; set; }
    
    [JsonConstructor]
    public QueueCompileRequest(Guid serviceId, int compileRequestId, string solution, string tests, bool isExercise)
    {
        ServiceId = serviceId;
        CompileRequestId = compileRequestId;
        Solution = solution;
        Tests = tests;
        IsExercise = isExercise;
    }
}