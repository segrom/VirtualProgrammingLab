using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Common;

namespace Common.Courses;

public class Impl
{
    [Key] public int Id { get; set; }

    [Required]
    public string TemplateCode { get; set; } // Rename to more correct TemplateCode
    [Required]
    public string TestsCode { get; set; }
    
    public int ExerciseId { get; set; }
    [ForeignKey("ExerciseId")] 
    public Exercise Exercise { get; set; }
    
    public int LanguageId { get; set; }
    [ForeignKey("LanguageId")] 
    public Language Language { get; set; }

    public Impl() {}

    public Impl(Language language, string templateCode, string testsCode, Exercise exercise)
    {
        TemplateCode = templateCode;
        TestsCode = testsCode;
        Exercise = exercise;
        ExerciseId = exercise.Id;
        Language = language;
        LanguageId = language.Id;
    }

    public bool HasPattern() => !string.IsNullOrEmpty(TemplateCode) && !TemplateCode.Equals(Language.DefaultTemplateCode);
    public bool HasTests() => !string.IsNullOrEmpty(TestsCode) && !TestsCode.Equals(Language.DefaultTestsCode);
}