using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Common;
using Application.Data.Students;

namespace Application.Data.Courses;

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
    
    public int LanguageId { get; set; }
    [ForeignKey("LanguageId")] 
    public Language Language { get; set; }

    public List<CompileRequest> CompileRequests { get; set; } = new();

    public ExerciseState() {}

    public ExerciseState(Student student, Language language)
    {
        Student = student;
        StudentId = student.Id;
        Language = language;
        LanguageId = language.Id;
        Status = ExerciseStatus.Started;
    }
}