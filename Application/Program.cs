using Application.Areas.Identity;
using Application.Controllers;
using Application.Middleware;
using Application.Services.Admin;
using Application.Services.Compile;
using Application.Services.Compile.Sandbox;
using Application.Services.Courses;
using Application.Services.Lecturers;
using Application.Services.Search;
using Application.Services.Students;
using Application.Services.Users;
using Common;
using Common.Account;
using Common.Common;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

bool isLocal = Environment.GetEnvironmentVariable("MODE") is null;
var connectionString = isLocal 
    ? builder.Configuration.GetConnectionString("DefaultConnection") 
    : Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING");

if (isLocal)
{
    Environment.SetEnvironmentVariable("DEFAULT_ADMIN_NAME", builder.Configuration.GetSection("env").GetValue<string>("DEFAULT_ADMIN_NAME"));
    Environment.SetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD", builder.Configuration.GetSection("env").GetValue<string>("DEFAULT_ADMIN_PASSWORD"));
}

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    if(isLocal)
        options.UseSqlite(connectionString, b => b.MigrationsAssembly("Application"));
    else
        options.UseNpgsql(connectionString);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 5;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddSession();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILecturerService, LecturerService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISandboxService, SandboxService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();

if(isLocal)
    builder.Services.AddSingleton<ICompileService, LocalCompileService>();
else
    builder.Services.AddSingleton<ICompileService, CompileService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");
app.UseMiddleware<BlazorCookieLoginMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider; 
    var context = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
    if(!isLocal) _ = services.GetRequiredService<ICompileService>();
    try
    {
        
        var userManager = services.GetRequiredService<UserManager<User>>();
        var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleController.InitializeAsync(userManager, rolesManager);
        
        if(!context.Languages.Any())
        {
            var languages = new[]
            {
                new Language("C#", "csharp")
                {
                    SandboxTemplateCode = 
                        @"namespace Exercise;

public class Solution
{
    public void Run()
    {
        // code here
    }
}",
                    DefaultTemplateCode = 
                        @"namespace Exercise;

public class Solution
{
    public int Sum(int x, int y)
    {
        return x * y;
    }
}",
                    DefaultTestsCode = 
                        @"namespace Exercise;

public class Tests
{
    // use ExerciseException
    public void Run(Solution solution)
    {
        var result = solution.Sum(10, 55);
        if (result != 65) throw new ExerciseException($""Sum of 10 and 55: answer = {65}, result = {result}"");
        
        result = solution.Sum(-2, 0);
        if (result != -2) throw new ExerciseException($""Sum of -2 and 0: answer = {-1}, result = {result}"");
    }
}"
                },
                new Language("Python 3.9", "py")
                {
                    SandboxTemplateCode = 
                        @"
class Solution:
    def run(self):
        pass",
                    DefaultTemplateCode = 
                        @"
class Solution:
    def calculateSum(self, x, y):
        return x + y",
                    DefaultTestsCode = 
                        @"from utils import *
from solution import *


class Tests:
    def run(self, solution: Solution):
        result = solution.calculateSum(-1, 15)
        if result != 14:
            raise ExerciseException(f""Sum of -1 and 15: answer = {14}, result = {result}"")

        result = solution.calculateSum(103, 15)
        if result != 25:
            raise ExerciseException(f""Sum of 10 and 15: answer = {25}, result = {result}"")

"
                },
                new Language("C++ 14", "cpp")
                {
                    SandboxTemplateCode = 
                        @"#pragma once

class Solution {
public:
    void Run() {
        // code
    }
};
",
                    DefaultTemplateCode = 
                        @"#pragma once

class Solution {
public:
    float Sum(float a, float b) {
        return a - b;
    }
};
",
                    DefaultTestsCode = 
                        @"#include ""Tests.h""
#include ""ExerciseException.h""

void Tests::Run(Solution* solution) {
    auto result = solution->Sum(10, 55);
    if (result != 65) throw new ExerciseException(""Sum of 10 and 55: answer = {65}, result = {result}"");

    result = solution->Sum(-2, 0);
    if (result != -2) throw new ExerciseException(""Sum of -2 and 0: answer = {-1}, result = {result}"");
}
"
                },
            };
            await context.Languages.AddRangeAsync(languages);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception e)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, "An error occurred while seeding the database.");
    }
}

app.Run();