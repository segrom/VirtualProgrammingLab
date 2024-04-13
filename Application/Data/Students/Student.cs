using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Account;
using Application.Data.Courses;

namespace Application.Data.Students;

public class Student
{
    [Key] public int Id { get; set; }
    
    public string UserId { get; set; }
    [Required][ForeignKey("UserId")]
    public User User { get; set; }
    
    public int GroupId { get; set; }
    [Required][ForeignKey("GroupId")]
    public StudentGroup Group { get; set; }

    public List<ChapterStudentState> ChapterStates { get; set; }

    public Student() { }
    
    public Student(User user, StudentGroup group)
    {
        GroupId = group.Id;
        Group = group;
        
        UserId = user.Id;
        User = user;
    }
}