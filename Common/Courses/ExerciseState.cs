using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Common;
using Common.Students;

namespace Common.Courses;

public enum ExerciseStatus
{
    Started, Failed, Completed
}

public class ExerciseState
{
    [Key] public int Id { get; set; }

    public ExerciseStatus Status { get; set; }
    
    public int StudentId { get; set; }
    [ForeignKey("StudentId")] 
    public Student Student { get; set; }
    
    public int ExerciseId { get; set; }
    [ForeignKey("ExerciseId")] 
    public Exercise Exercise { get; set; }
    
    public int ImplId { get; set; }
    [ForeignKey("ImplId")] 
    public Impl Impl { get; set; }

    public List<CompileRequest> CompileRequests { get; set; } = new();

    public ExerciseState() {}
}