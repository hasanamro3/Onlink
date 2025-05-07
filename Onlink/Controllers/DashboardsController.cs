// This is the recommended solution for handling Post creation
// based on your Post and Employee models.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Onlink.Data;
using Onlink.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Onlink.Controllers
{
    [Authorize]
    public class DashboardsController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;

        public DashboardsController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> CreatePost(Post model, IFormFile? MediaFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.Employee)
                .Include(u => u.Employer)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            if (user.UserType == "Employee" && user.Employee != null)
            {
                model.EmployeeId = user.Employee.EmployeeId;
            }
            else if (user.UserType == "Employer" && user.Employer != null)
            {
                model.EmployerId = user.Employer.EmployerId;
            }
            else
            {
                ModelState.AddModelError("", "No valid employee or employer found.");
                return View(model);
            }

            // Media handling
            if (MediaFile != null && MediaFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "posts");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(MediaFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await MediaFile.CopyToAsync(stream);

                model.MediaUrl = $"/uploads/posts/{fileName}";
                model.MediaType = MediaFile.ContentType.StartsWith("video") ? MediaType.Video : MediaType.Image;
            }
            else
            {
                model.MediaUrl = "";
                model.MediaType = MediaType.None;
            }

            model.CreatedAt = DateTime.UtcNow;

            _context.Post.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Posts));
        }

        [Route("")]
        [HttpGet]
        public IActionResult Posts()
        {
            var posts = _context.Post
                .Include(p => p.Employee)
                .Include(p => p.Employer)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(posts);
        }
    }
}
