using Application.Data.Account;
using Application.Data.Courses;
using Application.Data.Lecturers;
using Application.Data.Students;

namespace Application.Services.Lecturers;

public interface ILecturerService
{
    Task<Lecturer?> GetCurrentLecturerAsync();
    Task<List<Course>> GetLecturerCoursesAsync(Lecturer lecturer);
    Task<List<Course>> GetLecturerCoursesIncludeGroupsAsync(Lecturer lecturer);
    Task RemoveCourseFromGroup(Course course, StudentGroup group);
    Task AddCourseToGroup(Course course, StudentGroup group);
    Task AddCoursesToGroup(List<Course> courses, StudentGroup group);
}