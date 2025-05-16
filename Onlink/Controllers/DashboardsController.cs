using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Onlink.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Onlink.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        public async Task<IActionResult> CreateJob()
        {
            ViewBag.EmployerId = new SelectList(await _context.Employer.ToListAsync(), "EmployerId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("User not logged in.");

            var employer = await _context.Employer.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
                return BadRequest("Employer not found.");

            job.EmployerId = employer.EmployerId;
            job.CreatedAt = DateTime.UtcNow;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return RedirectToAction("Posts", "Dashboards");
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

            // ✅ Get the logged-in user ID from the authentication claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("User not logged in.");

            // ✅ Find the User in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // ✅ Link the post to either an Employee or Employer based on user type
            if (user.UserType == "Employee")
            {
                var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee == null) return BadRequest("Employee not found.");

                model.EmployeeId = employee.EmployeeId;
                model.EmployerId = null;
            }
            else if (user.UserType == "Employer")
            {
                var employer = await _context.Employer.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employer == null) return BadRequest("Employer not found.");

                model.EmployerId = employer.EmployerId;
                model.EmployeeId = null;
            }

            // ✅ Handle file upload
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
    // Get logged-in user ID
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        return Unauthorized();

    // Check if user already liked the post
    bool alreadyLiked = await _context.PostLikes
        .AnyAsync(pl => pl.PostId == id && pl.UserId == userId);

    if (alreadyLiked)
    {
        return Json(new { success = false, message = "You already liked this post." });
    }

    // Add like record
    _context.PostLikes.Add(new PostLike
    {
        PostId = id,
        UserId = userId
    });

    // Increase post's like count
    var post = await _context.Post.FindAsync(id);
    if (post == null) return NotFound();

    post.LikeCount++;

    await _context.SaveChangesAsync();

    return Json(new { success = true, likeCount = post.LikeCount });
}

        [HttpGet]
        public async Task<IActionResult> Jobs(string sort = "")
        {
            var jobsQuery = _context.Jobs.Include(j => j.Employer).AsQueryable();

            if (sort == "salary_desc")
                jobsQuery = jobsQuery.OrderByDescending(j => j.JobSalary);
            else if (sort == "salary_asc")
                jobsQuery = jobsQuery.OrderBy(j => j.JobSalary);

            var jobs = await jobsQuery.ToListAsync();

            // Get logged-in user type
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                ViewBag.UserType = user?.UserType;
            }

            ViewBag.Sort = sort;
            return View(jobs);
        }





    }
}