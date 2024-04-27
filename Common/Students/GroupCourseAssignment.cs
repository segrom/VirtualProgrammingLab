using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Account;
using Common.Courses;

namespace Common.Students;

public class GroupCourseAssignment
{
    [Key] public int Id { get; set; }
    
    public string AssignerId { get; set; }
    [Required][ForeignKey("AssignerId")]
    public User Assigner { get; set; }
    
    public int GroupId { get; set; }
    [Required][ForeignKey("GroupId")]
    public StudentGroup Group { get; set; }
    
    public int CourseId { get; set; }
    [Required][ForeignKey("CourseId")]
    public Course Course { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
    public DateTimeOffset ChangeDate { get; set; }

    public GroupCourseAssignment() { }
}