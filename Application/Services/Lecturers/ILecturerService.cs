using System.Text.RegularExpressions;
using Common.Courses;
using Common.Lecturers;
using Common.Students;

namespace Application.Services.Lecturers;

public interface ILecturerService
{
    Task<Lecturer?> GetCurrentLecturerAsync();
    Task<List<Course>> GetLecturerCoursesAsync(Lecturer lecturer);
    Task<List<Course>> GetLecturerCoursesIncludeGroupsAsync(Lecturer lecturer);
    Task RemoveCourseFromGroup(Course course, StudentGroup group);
    Task AddCourseToGroup(Course course, StudentGroup group);
    Task AddCoursesToGroup(List<Course> courses, StudentGroup group);
    Task DeleteChapterAsync(Chapter chapter);
    Task<List<StudentGroup>> GetAllGroups();
}