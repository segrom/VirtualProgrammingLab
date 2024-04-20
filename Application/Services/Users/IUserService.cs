using Common.Account;

namespace Application.Services.Users;

public interface IUserService
{
    Task<User?> GetCurrentUser();
}