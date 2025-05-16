using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Onlink.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Database context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")
    ?? throw new InvalidOperationException("Connection string 'DataContext' not found.")));

// ✅ Add MVC services
builder.Services.AddControllersWithViews();

// ✅ Cookie Authentication (manual, no Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "OnlinkAuthCookie";
        options.LoginPath = "/Accounts/Login";
        options.LogoutPath = "/Accounts/Logout";
        options.AccessDeniedPath = "/Accounts/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// ✅ Authorization services
builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Add these in correct order
app.UseAuthentication(); // ← MUST come before UseAuthorization
app.UseAuthorization();

// ✅ Default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Accounts}/{action=Login}/{id?}");

app.Run();
