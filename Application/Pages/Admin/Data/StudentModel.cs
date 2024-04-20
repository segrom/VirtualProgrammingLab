using System.ComponentModel.DataAnnotations;
using Common.Account;
using Common.Students;

namespace Application.Pages.Admin.Data;

public class StudentModel
{
    public User User => Student.User;
    [Required]
    public Student Student { get; set; }
    [StringLength(20, MinimumLength = 5 )]
    public string Password { get; set; }
}