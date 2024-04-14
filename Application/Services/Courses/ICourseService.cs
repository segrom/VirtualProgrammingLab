using Application.Data.Courses;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Courses;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(Course model);
}