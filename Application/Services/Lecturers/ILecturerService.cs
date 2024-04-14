using Application.Data.Account;
using Application.Data.Courses;
using Application.Data.Lecturers;

namespace Application.Services.Lecturers;

public interface ILecturerService
{
    Task<Lecturer?> GetCurrentLecturerAsync();
    Task<List<Course>> GetLecturerCoursesAsync(Lecturer lecturer);
}