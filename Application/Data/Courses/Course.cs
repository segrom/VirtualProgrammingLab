using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Lecturers;
using Application.Data.Students;

namespace Application.Data.Courses;

public enum CourseStatus
{
    InDevelop, Published, Deleted
}

public class Course
{
    [Key] public int Id { get; set; }

    [Required]
    public string Title { get; set; }
    public string Desc { get; set; }
    public CourseStatus Status { get; set; }
    
    public int AuthorId { get; set; }
    [ForeignKey("AuthorId")]
    public Lecturer Author { get; set; }
    
    public List<Chapter> Chapters { get; set; } = new();
    public List<StudentGroup> Groups { get; set; } = new();

    public Course() {}

    public Course(string title, string desc, Lecturer author)
    {
        Title = title;
        Desc = desc;
        Author = author;
        AuthorId = author.Id;
        Status = CourseStatus.InDevelop;
    }
}