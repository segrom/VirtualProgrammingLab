using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Account;
using Common.Courses;
using Common.Students;

namespace Common.Common;

public class SandboxState
{
    [Key] public int Id { get; set; }
    
    public string UserId { get; set; }
    [ForeignKey("UserId")] 
    public User User { get; set; }
    
    public int LanguageId { get; set; }
    [ForeignKey("LanguageId")] 
    public Language Language { get; set; }

    public string Name { get; set; }
    public string Code { get; set; }
    public DateTimeOffset CreationTime { get; set; }

    public List<CompileRequest> CompileRequests { get; set; } = new();
    
    
    public SandboxState()
    {
        CreationTime = DateTimeOffset.Now;
    }
}