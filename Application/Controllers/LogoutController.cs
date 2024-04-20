using Application.Data.Account;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Microsoft.AspNetCore.Components.Route("/logout")]
[ApiController]
public class LogoutController : Controller
{
    private readonly SignInManager<User> _signInManager;

    public LogoutController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost]
    [Microsoft.AspNetCore.Mvc.Route("/")]
    public async Task<IActionResult> Post()
    {
        await _signInManager.SignOutAsync();
        return new RedirectResult("/");
    }
}