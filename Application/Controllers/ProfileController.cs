using Common.Account;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Microsoft.AspNetCore.Components.Route("")]
public class ProfileController : Controller
{
    private readonly SignInManager<User> _signInManager;

    public ProfileController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Post()
    {
        Console.WriteLine($"Receivce post {Request.Path} | {Request.HttpContext.Session.Id} | {Request.Host}");
        await _signInManager.SignOutAsync();
        return new RedirectResult("/");
    }
}