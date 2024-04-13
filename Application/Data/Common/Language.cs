using System.ComponentModel.DataAnnotations;

namespace Application.Data.Common;

public class Language
{
    [Key] public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public List<CompileRequest> CompileRequests { get; set; } = new();

    public Language(string name)
    {
        Name = name;
    }
}