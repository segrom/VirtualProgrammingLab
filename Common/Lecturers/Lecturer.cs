using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Account;

namespace Common.Lecturers;

public class Lecturer
{
    [Key] public int Id { get; set; }

    public string? Faculty { get; set; }
    
    public string UserId { get; set; }
    [Required][ForeignKey("UserId")]
    public User User { get; set; }

    public Lecturer() { }

    public Lecturer(User user)
    {
        User = user;
        UserId = user.Id;
    }
}