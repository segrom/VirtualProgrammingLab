using Application.Data.Account;

namespace Application.Services.Users;

public interface IUserService
{
    Task<User?> GetCurrentUser();
}