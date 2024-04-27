using Common.Courses;
using Common.Students;

namespace Application.Services.Students;

public interface IStudentService
{
    Task<Student?> GetCurrentStudentAsync();
    Task<List<Course>> GetStudentCoursesAsync(Student student);
    Task<(int Finished,int All)> GetStudentCourseProgress(Student student, Course course);
    Task<(Chapter, ChapterStudentState)> UpdateChapterState(Chapter chapter, Student student);
    Task<List<ExerciseState>> GetExerciseStatesAsync(Exercise exercise, Student student);
    Task<Course> GetStudentCourseAsync(int courseId, Student s);
}