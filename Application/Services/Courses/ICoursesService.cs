using Application.Data.Courses;
using Application.Data.Students;

namespace Application.Services.Courses;

public interface ICoursesService
{
    List<Course> GetStudentCourses(Student student);
}