using Application.Data.Account;
using Application.Data.Common;
using Application.Data.Courses;
using Application.Data.Students;

namespace Application.Services.Students;

public interface IStudentService
{
    Task<Student?> GetCurrentStudentAsync();
    Task<List<Course>> GetStudentCoursesAsync(Student student);
    float GetStudentCourseProgress(Student student, Course course);
    Task<(Chapter, ChapterStudentState)> UpdateChapterState(Chapter chapter, Student student);
    Task<List<ExerciseState>> GetExerciseStatesAsync(Exercise exercise, Student student);
    Task<Course> GetStudentCourseAsync(int courseId, Student s);
}