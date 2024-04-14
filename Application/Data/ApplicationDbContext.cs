using Application.Data.Account;
using Application.Data.Common;
using Application.Data.Courses;
using Application.Data.Lecturers;
using Application.Data.Students;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Application.Data;

public sealed class ApplicationDbContext : IdentityDbContext<User> 
{
    
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<StudentGroup> StudentGroups { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Lecturer> Lecturers { get; set; } = null!;
    public DbSet<CompileRequest> CompileRequests { get; set; } = null!;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
}