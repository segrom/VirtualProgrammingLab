﻿using Common.Courses;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Courses;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(Course model);
    Task<Course> GetCourseAsync(int id);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourse(Course course);
    Task AddChapterToCourse(Course course, Chapter chapter);
    Task UpdateChapterAsync(Chapter chapter);
    Task<Exercise> GetExerciseAsync(Chapter chapter);
    Task ChangeCourseState(Course course, CourseStatus published);
}