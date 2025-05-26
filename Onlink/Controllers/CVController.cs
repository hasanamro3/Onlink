using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onlink.Data;
using Onlink.Models;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using Onlink.ML;
using Onlink.Services;
using UglyToad.PdfPig;
using System.Security.Claims;

namespace Onlink.Controllers
{
    public class CVController : Controller
    {
        private readonly DataContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly OpenAiService _openAi;

        public CVController(DataContext db, IWebHostEnvironment env, OpenAiService openAi)
        {
            _db = db;
            _env = env;
            _openAi = openAi;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompareCV(IFormFile cvFile, int jobId)
        {
            if (cvFile == null || cvFile.Length == 0)
                return BadRequest("CV file is required.");

            var jobApplication = await _db.JobApplication
                                          .Include(j => j.Job)
                                          .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (jobApplication == null || jobApplication.Job == null)
                return NotFound("Job not found.");

            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                await cvFile.CopyToAsync(stream);
            }

            string extractedText = Path.GetExtension(cvFile.FileName).ToLower() switch
            {
                ".pdf" => ExtractTextFromPdf(tempPath),
                ".docx" => ExtractTextFromDocx(tempPath),
                _ => throw new NotSupportedException("Only PDF and DOCX files are supported.")
            };

            // ML similarity
            double similarity = CalculateSimilarity(extractedText, jobApplication.Job.JobDescription);

            // AI skill extraction
            string aiPrompt = $"Extract top 5 technical skills from this resume:\n\n{extractedText}";
            string aiSkills = await _openAi.AskGPT(aiPrompt);

            // Save to database (if user is known)
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var resume = await _db.Resume.FirstOrDefaultAsync(r => r.Employee != null && r.Employee.UserId == userId);
            if (resume != null)
            {
                resume.Skills = aiSkills;
                _db.Resume.Update(resume);
                await _db.SaveChangesAsync();
            }

            var result = new CVComparisonResult
            {
                FileName = cvFile.FileName,
                SimilarityScore = Math.Round(similarity * 100, 2),
                MatchedJobTitle = jobApplication.Job.JobName
            };

            ViewBag.AISkills = aiSkills;
            return View("Result", result);
        }

        private static string ExtractTextFromPdf(string path)
        {
            using var document = PdfDocument.Open(path);
            var allText = new List<string>();
            foreach (var page in document.GetPages())
            {
                allText.Add(page.Text);
            }
            return string.Join(" ", allText);
        }

        private static string ExtractTextFromDocx(string path)
        {
            using var wordDoc = WordprocessingDocument.Open(path, false);
            return wordDoc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
        }

        private static double CalculateSimilarity(string text1, string text2)
        {
            var vector1 = GetWordVector(text1);
            var vector2 = GetWordVector(text2);

            double dotProduct = 0, mag1 = 0, mag2 = 0;

            foreach (var word in vector1.Keys)
            {
                if (vector2.ContainsKey(word))
                    dotProduct += vector1[word] * vector2[word];

                mag1 += Math.Pow(vector1[word], 2);
            }

            foreach (var val in vector2.Values)
                mag2 += Math.Pow(val, 2);

            mag1 = Math.Sqrt(mag1);
            mag2 = Math.Sqrt(mag2);

            return (mag1 * mag2 == 0) ? 0 : dotProduct / (mag1 * mag2);
        }

        private static Dictionary<string, int> GetWordVector(string text)
        {
            var words = Regex.Split(text.ToLower(), @"\W+")
                             .Where(w => !string.IsNullOrWhiteSpace(w));

            var dict = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (!dict.ContainsKey(word))
                    dict[word] = 0;
                dict[word]++;
            }

            return dict;
        }

        [HttpGet]
        public async Task<IActionResult> PredictMatch(int resumeId, int jobApplicationId)
        {
            var resume = await _db.Resume.FindAsync(resumeId);
            var jobApp = await _db.JobApplication
                                  .Include(j => j.Job)
                                  .FirstOrDefaultAsync(j => j.JobApplicationId == jobApplicationId);

            if (resume == null || jobApp == null || jobApp.Job == null)
                return NotFound();

            var result = MLModel.Predict(new ModelInput
            {
                ResumeText = $"{resume.Summary} {resume.Skills} {resume.Experience} {resume.Education}",
                JobDescription = $"{jobApp.Job.JobName} {jobApp.Job.JobDescription} {jobApp.Job.JobSalary} {jobApp.Job.SubmitSessionDueDate}"
            });

            ViewBag.Prediction = result.Prediction;
            ViewBag.Score = Math.Round(result.Score * 100, 2);
            ViewBag.ResumeName = resume.FullName;
            ViewBag.JobTitle = jobApp.Job.JobName;

            return View("PredictionResult");
        }

        public async Task<IActionResult> AIExplain(int resumeId)
        {
            var resume = await _db.Resume.FindAsync(resumeId);
            if (resume == null) return NotFound();

            var prompt = $"Summarize this resume and suggest job roles:\n\n{resume.Summary} {resume.Experience} {resume.Skills}";
            var result = await _openAi.AskGPT(prompt);

            ViewBag.AIResponse = result;
            return View("AIResult");
        }

        [HttpGet]
        public async Task<IActionResult> SuggestedJobs()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userType = User.FindFirst("UserType")?.Value;

            if (userIdClaim == null || userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Accounts"); // Or Forbid()
            }

            int userId = int.Parse(userIdClaim.Value);

            var employee = await _db.Employee
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }

            var resume = await _db.Resume.FirstOrDefaultAsync(r => r.EmployeeId == employee.EmployeeId);
            if (resume == null)
            {
                ViewBag.Message = "You need to upload a resume first.";
                return View("SuggestedJobs", new List<(Job, double)>());
            }

            string resumeText = $"{resume.Summary} {resume.Experience} {resume.Education} {resume.Skills}";

            var jobs = await _db.Jobs.ToListAsync();
            var results = new List<(Job job, double score)>();

            foreach (var job in jobs)
            {
                string jobText = $"{job.JobName} {job.JobDescription}";
                double score = CVController.CalculateSimilarity(resumeText, jobText);

                if (score > 0.2) // Adjust threshold as needed
                {
                    results.Add((job, Math.Round(score * 100, 2)));
                }
            }

            return View("SuggestedJobs", results.OrderByDescending(r => r.score).ToList());
        }
    }
}
