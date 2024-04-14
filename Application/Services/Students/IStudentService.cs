using Application.Data.Account;
using Application.Data.Courses;
using Application.Data.Students;

namespace Application.Services.Students;

public interface IStudentService
{
    Task<Student?> GetCurrentStudentAsync();
    Task<List<Course>> GetStudentCoursesAsync(Student student);
    float GetStudentCourseProgressAsync(Student student, Course course);
}