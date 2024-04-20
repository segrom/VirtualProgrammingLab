using Application.Pages.Admin;
using Application.Pages.Admin.Data;
using Common.Account;
using Common.Common;
using Common.Students;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Admin;

public interface IAdminService
{
    Task<StudentGroup[]?> GetAllStudentGroupsAsync();
    Task<bool> DeleteGroupAsync(StudentGroup group);
    Task CreateOrUpdateGroupAsync(StudentGroup group);
    Task<IdentityResult> UpdateLecturerWithUserAsync(LecturerModel model);
    Task<IdentityResult> CreateLecturerWithUserAsync(LecturerModel model);
    Task<IdentityResult> CreateStudentWithUserAsync(StudentModel model);
    Task<IdentityResult> UpdateStudentWithUserAsync(StudentModel model);
    Task<List<CompileRequest>> GetUserCompileRequests(User u);
}