using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Data.Courses;

public class Exercise
{
    [Key] public int Id { get; set; }

    public List<Impl> Implementations { get; set; } = new();
    
    public int ChapterId { get; set; }
    [ForeignKey("ChapterId")] 
    public Chapter Chapter { get; set; }

    public Exercise() {}

    public Exercise(Chapter chapter)
    {
        Chapter = chapter;
        ChapterId = chapter.Id;
    }
}