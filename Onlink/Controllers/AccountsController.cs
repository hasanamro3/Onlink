using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onlink.Data;
using Onlink.Models;
using System.Security.Claims;

[Authorize]
public class AccountsController : Controller
{
    private readonly DataContext _db;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(DataContext db, ILogger<AccountsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out int userId))
            return userId;

        throw new UnauthorizedAccessException("User is not authenticated.");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new("UserType", user.UserType)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();

        var user = await _db.Users
            .Include(u => u.Employee).ThenInclude(e => e.Certificates)
            .Include(u => u.Employee).ThenInclude(e => e.Resumes)
            .Include(u => u.Employee).ThenInclude(e => e.JobApplications)
            .Include(u => u.Employer).ThenInclude(e => e.Jobs)
            .Include(u => u.Employer).ThenInclude(e => e.Resume)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return Forbid();

        ViewData["UserType"] = user.UserType;
        ViewData["ProfilePicturePath"] = user.ProfilePicturePath ?? "/images/default-user.png";

        if (user.UserType == "Employee" && user.Employee == null) return Forbid();
        if (user.UserType == "Employer" && user.Employer == null) return Forbid();

        return View(user);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _db.Users.AnyAsync(u => u.UserName == model.Username))
        {
            ModelState.AddModelError("Username", "Username is already taken.");
            return View(model);
        }

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email is already registered.");
            return View(model);
        }

        var user = new User
        {
            UserName = model.Username,
            Email = model.Email,
            UserType = model.UserType,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            ConfirmPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (model.UserType == "Employee")
        {
            _db.Employee.Add(new Employee
            {
                FirstName = model.Username.Split(' ')[0],
                LastName = model.Username.Split(' ').Length > 1 ? model.Username.Split(' ')[1] : "",
                Email = model.Email,
                UserId = user.UserId
            });
        }
        else if (model.UserType == "Employer")
        {
            _db.Employer.Add(new Employer
            {
                Name = model.Username,
                Email = model.Email,
                UserId = user.UserId
            });
        }

        await _db.SaveChangesAsync();

        // Auto-login
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new("UserType", user.UserType)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        var user = await _db.Users
            .Include(u => u.Employee)
            .Include(u => u.Employer)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        ViewData["UserType"] = user.UserType;
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model, IFormFile? ProfilePictureFile)
    {
        var userId = GetCurrentUserId();
        var user = await _db.Users
            .Include(u => u.Employee)
            .Include(u => u.Employer)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        user.UserName = model.UserName;
        user.Email = model.Email;

        if (user.UserType == "Employee" && model.Employee != null)
        {
            user.Employee.FirstName = model.Employee.FirstName;
            user.Employee.PhoneNumber = model.Employee.PhoneNumber;
        }
        else if (user.UserType == "Employer" && model.Employer != null)
        {
            user.Employer.Name = model.Employer.Name;
            user.Employer.PhoneNumber = model.Employer.PhoneNumber;
        }

        if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(ProfilePictureFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProfilePictureFile", "Invalid image file.");
                return View(user);
            }

            var uploads = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploads);

            var uniqueName = $"user_{user.UserId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploads, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePictureFile.CopyToAsync(stream);
            }

            user.ProfilePicturePath = $"/uploads/{uniqueName}";
        }

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("Profile");
    }

    [HttpGet]
    public IActionResult ChangePassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetCurrentUserId();
        var user = await _db.Users.FindAsync(userId);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            return View(model);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _db.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Password changed. Please log in again.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Error() => View();
    
}
