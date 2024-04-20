using System.ComponentModel.DataAnnotations;
using Common.Account;
using Common.Lecturers;

namespace Application.Pages.Admin.Data;

public class LecturerModel
{
    public User User => Lecturer.User;
    [Required]
    public Lecturer Lecturer { get; set; }
    [StringLength(20, MinimumLength = 5 )]
    public string Password { get; set; }
}