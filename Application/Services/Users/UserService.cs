using Application.Data;
using Application.Data.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Users;

public class UserService: IUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<User> _userManager;
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private ILogger<UserService> _logger;

    public UserService(AuthenticationStateProvider authenticationStateProvider, 
        IDbContextFactory<ApplicationDbContext> dbContextFactory, 
        ILogger<UserService> logger, UserManager<User> userManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _userManager = userManager;
    }
    
    private User? _user;
    private bool _busy;

    public async Task<User?> GetCurrentUser()
    {
        if (_busy)
        {
            while (_busy)
            {
                await Task.Yield();
            }
        }

        if (_user is not null) return _user;
        
        _busy = true;
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        _user = await _userManager.GetUserAsync(state.User);
        _busy = false;
        return _user;
    }
}