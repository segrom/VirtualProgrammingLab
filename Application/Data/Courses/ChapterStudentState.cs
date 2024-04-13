using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Students;

namespace Application.Data.Courses;

public class ChapterStudentState
{
    [Key] public int Id { get; set; }

    public DateTimeOffset FirstOpenTime { get; set; }
    public DateTimeOffset LastOpenTime { get; set; }
    public int OpeningsCount { get; set; }
    
    public int StudentId { get; set; }
    [ForeignKey("StudentId")] 
    public Student Student { get; set; }
    
    public int ChapterId { get; set; }
    [ForeignKey("ChapterId")] 
    public Chapter Chapter { get; set; }

    public ChapterStudentState() { }

    public ChapterStudentState(Student student, Chapter chapter)
    {
        Student = student;
        StudentId = student.Id;
        Chapter = chapter;
        ChapterId = chapter.Id;
        FirstOpenTime = DateTimeOffset.Now;
        LastOpenTime = DateTimeOffset.Now;

        OpeningsCount = 1;
    }
}