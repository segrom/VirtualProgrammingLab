using Application.Data;
using Application.Data.Courses;
using Application.Data.Students;

namespace Application.Services.Courses;

public class CoursesService : ICoursesService
{
    private ApplicationDbContext _dbContext;
    private ILogger<CoursesService> _logger;

    public CoursesService(ApplicationDbContext dbContext, ILogger<CoursesService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public List<Course> GetStudentCourses(Student student)
    {
        throw new NotImplementedException();
    }
}