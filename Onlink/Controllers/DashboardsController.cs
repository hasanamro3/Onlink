using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Onlink.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Onlink.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Onlink.Controllers
{
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
        public async Task<IActionResult> CreateJob()
        {
            ViewBag.EmployerId = new SelectList(await _context.Employer.ToListAsync(), "EmployerId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Job.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EmployerId = new SelectList(await _context.Employer.ToListAsync(), "EmployerId", "Name", job.EmployerId);
            return View(job);
        }


        // GET: Show the form to create a post
        [HttpGet]
        public IActionResult CreatePost()
        {
            return View(new Post());
        }

        // POST: Save the post
        [HttpPost]
        [ValidateAntiForgeryToken]
    
        public async Task<IActionResult> CreatePost(Post model, IFormFile? MediaFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Simulate logged-in user (replace with actual user logic)
            var userId = 1; // Replace with actual user ID from auth/session
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Set Employee or Employer ID
            if (user.UserType == "Employee")
            {
                var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee != null)
                {
                    model.EmployeeId = employee.EmployeeId;
                    model.EmployerId = 0;
                }
            }
            else if (user.UserType == "Employer")
            {
                var employer = await _context.Employer.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employer != null)
                {
                    model.EmployerId = employer.EmployerId;
                    model.EmployeeId = 0;
                }
            }

            // Handle media upload (Image/Video)
            if (MediaFile != null && MediaFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "posts");
                Directory.CreateDirectory(uploadsDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(MediaFile.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await MediaFile.CopyToAsync(stream);
                }

                model.MediaUrl = "/uploads/posts/" + fileName;
                model.MediaType = MediaFile.ContentType.StartsWith("video") ? MediaType.Video : MediaType.Image;
            }
            else
            {
                model.MediaUrl = string.Empty;
                model.MediaType = MediaType.None;
            }

            model.CreatedAt = DateTime.UtcNow;

            _context.Post.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Posts));
        }

        // GET: Display all posts
        [HttpGet]
        public async Task<IActionResult> Posts()
        {
            var posts = await _context.Post
                .Include(p => p.Employee)
                .Include(p => p.Employer)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int id)
        {
            var post = await _context.Post.FindAsync(id);
            if (post == null) return NotFound();

            post.LikeCount++;
            await _context.SaveChangesAsync();

            return Json(new { success = true, likeCount = post.LikeCount });
        }

    }
}
