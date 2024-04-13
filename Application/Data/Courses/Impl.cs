using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Common;

namespace Application.Data.Courses;

public class Impl
{
    [Key] public int Id { get; set; }

    [Required]
    public string PatternCode { get; set; }
    [Required]
    public string TestsCode { get; set; }
    
    public int ExerciseId { get; set; }
    [ForeignKey("ExerciseId")] 
    public Exercise Exercise { get; set; }
    
    public int LanguageId { get; set; }
    [ForeignKey("LanguageId")] 
    public Language Language { get; set; }

    public Impl() {}

    public Impl(Language language, string patternCode, string testsCode, Exercise exercise)
    {
        PatternCode = patternCode;
        TestsCode = testsCode;
        Exercise = exercise;
        ExerciseId = exercise.Id;
        Language = language;
        LanguageId = language.Id;
    }
}