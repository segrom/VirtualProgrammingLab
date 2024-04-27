using Common.Account;
using Common.Common;
using Common.Courses;
using Common.Lecturers;
using Common.Students;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Common;

public sealed class ApplicationDbContext : IdentityDbContext<User> 
{
    
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Chapter> Chapters { get; set; } = null!;
    public DbSet<StudentGroup> StudentGroups { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Lecturer> Lecturers { get; set; } = null!;
    public DbSet<CompileRequest> CompileRequests { get; set; } = null!;
    public DbSet<Language> Languages { get; set; } = null!;
    public DbSet<ChapterStudentState> ChapterStudentStates { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<ExerciseState> ExerciseStates { get; set; } = null!;
    public DbSet<Impl> Impls { get; set; } = null!;
    public DbSet<GroupCourseAssignment> Assignments { get; set; } = null!;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
}