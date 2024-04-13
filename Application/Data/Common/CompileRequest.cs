using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Account;
using Application.Data.Courses;

namespace Application.Data.Common;

public enum CompileRequestStatus
{
    Queued, Finished, FinishedWithErrors, Failed
}

public class CompileRequest
{
    [Key] public int Id { get; set; }

    [Required]
    public string Code { get; set; }
    public string? Output { get; set; }
    public string? Errors { get; set; }
    public CompileRequestStatus Status { get; set; }

    public DateTimeOffset CreationTime { get; set; }
    public DateTimeOffset? FinishTime { get; set; }
    
    public string UserId { get; set; }
    [Required][ForeignKey("UserId")]
    public User User { get; set; }
    
    public int LanguageId { get; set; }
    [Required][ForeignKey("LanguageId")] 
    public Language Language { get; set; }
    
    public int? ExerciseStateId { get; set; }
    [ForeignKey("ExerciseStateId")] 
    public ExerciseState? ExerciseState { get; set; }

    public CompileRequest() { }

    public CompileRequest(string code, User user, Language language)
    {
        Code = code;
        User = user;
        UserId = user.Id;
        Language = language;
        LanguageId = language.Id;
        CreationTime = DateTimeOffset.Now;
    }
    
    public CompileRequest(string code, User user, Language language, ExerciseState exerciseState)
    {
        Code = code;
        User = user;
        UserId = user.Id;
        Language = language;
        LanguageId = language.Id;
        CreationTime = DateTimeOffset.Now;
        ExerciseState = exerciseState;
        ExerciseStateId = exerciseState.Id;
    }
}