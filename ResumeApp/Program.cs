using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.Services;

var builder = WebApplication.CreateBuilder(args);
//license context to non-commercial
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024; // 500 MB
});
//  DbContext with connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Identity services with custom user model
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//Configure authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ActivityLogger>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMatchScoringService, MatchScoringService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();
//  Role Seeding
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();

    string[] roles = { "Admin", "Reviewer", "User", "Team Lead", "Manager", "Vendor", "Panel" };

    foreach (var role in roles)
    {
        var exists = await roleManager.RoleExistsAsync(role);
        if (!exists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    // Only for test environment - creating some test users
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        async Task CreateUserIfMissing(string email, string displayName, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var newUser = new Users
                {
                    UserName = email,
                    Email = email,
                    FullName = displayName,
                    EmailConfirmed = true
                };
                var res = await userManager.CreateAsync(newUser, password);
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }
                else
                {
                    // optional: log creation errors
                }
            }
            else
            {
                // ensure role assigned
                if (!await userManager.IsInRoleAsync(user, role))
                    await userManager.AddToRoleAsync(user, role);
            }
        }

        // Example test accounts
        await CreateUserIfMissing("teamlead@xeedo.in", "Team Lead", "Xeedo@123", "Team Lead");
        await CreateUserIfMissing("manager@xeedo.in", "Manager", "Xeedo@123", "Manager");
        await CreateUserIfMissing("vendor@xeedo.in", "Vendor", "Xeedo@123", "Vendor");
        await CreateUserIfMissing("panel@xeedo.in", "Panel", "Xeedo@123", "Panel");
    }

    //  Create a default admin user
    string adminEmail = "admin@xeedo.in";
    string adminPassword = "Admin@123";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new Users
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Admin"
        };
        var createResult = await userManager.CreateAsync(newAdmin, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

//Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

//default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
