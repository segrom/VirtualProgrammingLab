using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Lecturers;
using Common.Students;

namespace Common.Courses;

public enum CourseStatus
{
    InDevelop, Published, Deleted
}

public class Course
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Курсу небходимо название")]
    [MinLength(3, ErrorMessage = "Название не может быть короче 3х символов")]
    public string Title { get; set; }
    [Required(ErrorMessage = "Описание тоже не должно быть пустым")]
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