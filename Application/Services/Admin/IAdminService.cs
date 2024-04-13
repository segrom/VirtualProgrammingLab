using Application.Data.Students;
using Application.Pages.Admin;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Admin;

public interface IAdminService
{
    Task<StudentGroup[]?> GetAllStudentGroupsAsync();
    Task<bool> DeleteGroupAsync(StudentGroup group);
    Task CreateOrUpdateGroupAsync(StudentGroup group);
    Task<IdentityResult> UpdateLecturerWithUserAsync(AdminLecturerModal.LecturerModel model);
    Task<IdentityResult> CreateLecturerWithUserAsync(AdminLecturerModal.LecturerModel model);
    Task<IdentityResult> CreateStudentWithUserAsync(AdminStudentModal.StudentModel model);
    Task<IdentityResult> UpdateStudentWithUserAsync(AdminStudentModal.StudentModel model);
}