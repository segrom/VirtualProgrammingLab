using System.ComponentModel.DataAnnotations;

namespace Application.Data.Common;

public class Language
{
    [Key] public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    [Required]
    public string HighlightLabel { get; set; }
    
    public string? DefaultTemplateCode { get; set; }
    public string? DefaultTestsCode { get; set; }

    public List<CompileRequest> CompileRequests { get; set; } = new();


    public Language() { }

    public Language(string name, string highlightLabel)
    {
        Name = name;
        HighlightLabel = highlightLabel;
    }
}