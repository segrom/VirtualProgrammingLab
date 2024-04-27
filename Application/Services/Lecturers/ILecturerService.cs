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
    Task<GroupCourseAssignment> AddCourseToGroup(Course course, StudentGroup group, Lecturer lecturer);
    Task DeleteChapterAsync(Chapter chapter);
    Task<List<StudentGroup>> GetAllGroups();
}