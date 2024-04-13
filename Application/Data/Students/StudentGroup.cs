using System.ComponentModel.DataAnnotations;
using Application.Data.Courses;

namespace Application.Data.Students;

public class StudentGroup
{
    [Key] public int Id { get; set; }
    
    [Required]
    [MinLength(3)]
    public string Name { get; set; }

    public List<Student> Students { get; set; } = new();
    public List<Course> Courses { get; set; } = new();

    public StudentGroup() {}

    public StudentGroup(string name)
    {
        Name = name;
    }
}