using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Utility;
using InventoryWeb.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Postgres
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayDb")));




// Automaticall added by Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();



// Add Razor Pages Support for Identity Pages
builder.Services.AddRazorPages();


// set default paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";


    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);   // cookie lifetime
    options.SlidingExpiration = true;                    // resets timer on activity
    options.Events.OnRedirectToLogin = context =>
    {
        // optional: prevent redirect loops for APIs
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };

});


//Including the Repo pattern service
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
builder.Services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
builder.Services.AddScoped<ICarouselRepository, CarouselRepository>();
builder.Services.AddScoped<IRestaurantOrderRepository, RestaurantOrderRepository>();



builder.Services.AddScoped<IEmailSender, EmailSender>();




// When user role is changed or user is deleted, next request will logout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var identityOptions = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<IdentityOptions>>().Value;

            var userId = userManager.GetUserId(context.Principal);

            // if cookie somehow has no ID, reject immediately, Cookie tampered
            if (userId == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return;
            }

            // get DB user
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return;
            }

            // get security stamp claim in cookie
            string stampClaimType = identityOptions.ClaimsIdentity.SecurityStampClaimType;
            string principalStamp = context.Principal.FindFirstValue(stampClaimType);

            // get stamp from database
            string dbStamp = await userManager.GetSecurityStampAsync(user);

            // if stamp mismatch => user roles were changed or password reset
            if (principalStamp == null || dbStamp == null || principalStamp != dbStamp)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return;
            }
        }
    };
});


builder.Services.AddSession();



// building
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Added after Identity
app.UseAuthentication();

app.UseAuthorization();

// For Razor Pages
app.MapRazorPages();


app.UseSession();
// Middleware for One Login at a Time, if the sessionId in the cookie !== CurrentSessionId in db, close session.
app.UseMiddleware<SessionValidationMiddleware>();




//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/")
//    {
//        context.Response.Redirect("/Identity/Account/Login");
//        return;
//    }
//    await next();
//});
app.MapControllerRoute(
    name: "default",
    pattern: "/{controller=Home}/{action=Index}/{id?}");
//asp - area = "Identity" asp - page = "/Account/Login"




// Create Roles and Default Admin User
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Ensure Roles Exist
    string[] roles = { SD.Role_Admin, SD.Role_Employee };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 2. Ensure Default Admin User Exists
    string adminUserName = "1111111111";      // username
    string adminPassword = "$Admin1234";   // password (must follow Identity rules)

    var adminUser = await userManager.FindByNameAsync(adminUserName);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminUserName,
            PhoneNumber = adminUserName,
            //Email = "admin@inventoryweb.com",
            //EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Admin user creation failed: {error.Description}");
            }
        }
    }
}




app.Run();
