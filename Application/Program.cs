using Application.Areas.Identity;
using Application.Data;
using Application.Data.Account;
using Application.Data.Common;
using Application.Middleware;
using Application.Services.Admin;
using Application.Services.Compile;
using Application.Services.Courses;
using Application.Services.Lecturers;
using Application.Services.Search;
using Application.Services.Students;
using Application.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
        options.UseNpgsql(connectionString);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 5;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
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
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddSingleton<ICompileService, CompileService>();

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
app.UseMiddleware<BlazorCookieLoginMiddleware>();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
        /*await context.Database.EnsureCreatedAsync();
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }*/
        var userManager = services.GetRequiredService<UserManager<User>>();
        var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleController.InitializeAsync(userManager, rolesManager);
        
        if(!context.Languages.Any())
        {
            var languages = new[]
            {
                new Language("C#", "csharp")
                {
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
                new Language("Python 3", "py"),
                new Language("C++", "cpp"),
            };
            await context.Languages.AddRangeAsync(languages);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();