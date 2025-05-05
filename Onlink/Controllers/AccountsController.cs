using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onlink.Models;
using System.Security.Claims;
using System;
using Onlink.Data;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.IdentityModel.Tokens;

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
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
        if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
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

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

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

   

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        if (userId==null) return Forbid();

        var userType = User.FindFirstValue("UserType");
        if (string.IsNullOrEmpty(userType)) return Forbid();

        // Load user with all related data in one query
        var user = await _db.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e.Certificates)
            .Include(u => u.Employee)
                .ThenInclude(e => e.Resumes)
            .Include(u => u.Employee)
                .ThenInclude(e => e.JobApplications)
                    .ThenInclude(ja => ja.JobApplication)
            .Include(u => u.Employer)
                .ThenInclude(e => e.Jobs)
            .Include(u => u.Employer)
                .ThenInclude(e => e.Resume)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return Forbid();

        // Set view data for the profile picture
        ViewData["UserType"] = userType;
        ViewData["ProfilePicturePath"] = user.ProfilePicturePath ?? "/images/default-user.png";

        // Verify user type matches loaded data
        if (userType == "Employee" && user.Employee == null)
        {
            return Forbid();
        }
        else if (userType == "Employer" && user.Employer == null)
        {
            return Forbid();
        }

        return View(user);
    }


    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
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
            PasswordHash = HashPassword(model.Password),
            ConfirmPasswordHash= HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Create user-specific profile based on type
        switch (model.UserType)
        {
            case "Employee":
                var employee = new Employee
                {
                    FirstName = model.Username.Split(' ')[0],
                    LastName = model.Username.Split(' ').Length > 1 ? model.Username.Split(' ')[1] : "",
                    Email = model.Email,
                    UserId = user.UserId
                };
                _db.Employee.Add(employee);
                break;
            case "Employer":
                var employer = new Employer
                {
                    Name = model.Username,
                    Email = model.Email,
                    UserId = user.UserId
                };
                _db.Employer.Add(employer);
                break;
        }

        await _db.SaveChangesAsync();

        // Sign in the newly registered user
        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new(ClaimTypes.Name, user.UserName),
        new(ClaimTypes.Email, user.Email),
        new("UserType", user.UserType)
    };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }
    // GET: Edit Profile
    [HttpGet]
    // GET: /Accounts/Edit
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

    // POST: /Accounts/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model, IFormFile? ProfilePictureFile)
    {
        var userId = GetCurrentUserId();
        var user = await _db.Users
            .Include(u => u.Employee)
            .Include(u => u.Employer)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        // Update shared User fields
        user.UserName = model.UserName;
        user.Email = model.Email;

        // Update Employee or Employer-specific fields
        if (user.UserType == "Employee" && user.Employee != null && model.Employee != null)
        {
            user.Employee.FirstName = model.Employee.FirstName;
            user.Employee.PhoneNumber = model.Employee.PhoneNumber;
        }
        else if (user.UserType == "Employer" && user.Employer != null && model.Employer != null)
        {
            user.Employer.Name = model.Employer.Name;
            user.Employer.PhoneNumber = model.Employer.PhoneNumber;
        }

        // Handle profile picture upload
        if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(ProfilePictureFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProfilePictureFile", "Only JPG, JPEG, and PNG files are allowed.");
                ViewData["UserType"] = user.UserType;
                return View(user);
            }

            if (ProfilePictureFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ProfilePictureFile", "File size exceeds 5MB limit.");
                ViewData["UserType"] = user.UserType;
                return View(user);
            }

            var uploadsFolder = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var oldPath = Path.Combine("wwwroot", user.ProfilePicturePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            var uniqueFileName = $"user_{user.UserId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePictureFile.CopyToAsync(fileStream);
            }

            user.ProfilePicturePath = $"/uploads/{uniqueFileName}";
        }

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetCurrentUserId();
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            return View(model);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _db.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["SuccessMessage"] = "Your password has been changed successfully. Please log in again.";
        return RedirectToAction(nameof(Login));
    }
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    [HttpGet]
    public IActionResult Posts()
    {
        return View();
    }
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}