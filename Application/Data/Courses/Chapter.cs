using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Students;

namespace Application.Data.Courses;

public class Chapter
{
    [Key] public int Id { get; set; }
    
    [Required]
    public string Title { get; set; }
    [Range(0, 1000)]
    public float? Duration { get; set; }
    public string Body { get; set; }
    
    [Required]
    public bool IsExercise { get; set; }
    public Exercise? Exercise { get; set; }
    
    public int CourseId { get; set; }
    [Required][ForeignKey("CourseId")] 
    public Course Course { get; set; }

    public List<ChapterStudentState> StudentStates { get; set; } = new();

    public Chapter() { }

    public Chapter(string title, string body, Course course)
    {
        Title = title;
        Body = body;
        Course = course;
        CourseId = course.Id;
    }

    public ChapterStudentState? StudentState(Student student)
    {
        return StudentStates?.FirstOrDefault(s => s.StudentId == student.Id);
    }
}