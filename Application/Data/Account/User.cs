using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Data.Lecturers;
using Application.Data.Students;
using Microsoft.AspNetCore.Identity;

namespace Application.Data.Account;

public class User: IdentityUser
{
    [NotMapped] public string Code => UserName;

    [Required][ProtectedPersonalData]
    public virtual string Name { get; set; }
    
    [Required][ProtectedPersonalData]
    public virtual string Surname { get; set; }
    
    [Required][ProtectedPersonalData]
    public virtual string Patronymic { get; set; }
    
    public virtual DateTimeOffset CreationDate { get; set; }
    public virtual DateTimeOffset ActivityDate { get; set; }
    public virtual int CompileRequests { get; set; }
    
    public Lecturer? Lecturer { get; set; }
    public Student? Student { get; set; }

    public User()
    {
        CreationDate = DateTimeOffset.Now;
        ActivityDate = CreationDate;
    }

    public User(string code, string name, string surname, string patronymic) : base(code)
    {
        Name = name;
        Surname = surname;
        Patronymic = patronymic;
        CreationDate = DateTimeOffset.Now;
        ActivityDate = CreationDate;
    }

    public string GetShortName() => $"{Surname} {Name[0]}. {Patronymic[0]}.";

    public string GetFullName() => $"{Surname} {Name} {Patronymic}";
}