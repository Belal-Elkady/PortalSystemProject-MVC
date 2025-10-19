using BL.Contracts;
using BL.Dtos;
using Domains.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PortalSystemProject.Controllers
{
    [Authorize]
    public class JobSeekerProfileController : Controller
    {
        private readonly IJobSeekerProfileRepository _jobSeekerRepo;
        private readonly ILogger<JobSeekerProfileController> _logger;
        private readonly UserManager<ApplicationUser> _userManager; // ✅ add this

        public JobSeekerProfileController(
            IJobSeekerProfileRepository jobSeekerRepo,
            ILogger<JobSeekerProfileController> logger,
            UserManager<ApplicationUser> userManager // ✅ inject here
        )
        {
            _jobSeekerRepo = jobSeekerRepo;
            _logger = logger;
            _userManager = userManager; // ✅ assign
        }

        // GET: JobSeekerProfile
        public IActionResult Index()
        {
            var seekers = _jobSeekerRepo.GetAll();
            return View(seekers);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobSeekerProfileDto dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "You must be logged in to create a profile.";
                    return RedirectToAction("Login", "Account");
                }

                dto.UserId = user.Id;
                dto.CreatedAt = DateTime.Now;
                _jobSeekerRepo.Add(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }


        // GET: Edit
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var seeker = _jobSeekerRepo.GetById(id);
            if (seeker == null)
                return NotFound();
            return View(seeker);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, JobSeekerProfileDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var existing = _jobSeekerRepo.GetById(id);
            if (existing == null)
                return NotFound();

            // Preserve critical values
            dto.UserId = existing.UserId;
            dto.CreatedAt = existing.CreatedAt;


            if (ModelState.IsValid)
            {
                try
                {
                    _jobSeekerRepo.Update(dto);
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating JobSeeker profile");
                    ModelState.AddModelError("", "An unexpected error occurred while updating.");
                }
            }

            return View(dto);
        }


        // GET: Details
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var seeker = _jobSeekerRepo.GetById(id);
            if (seeker == null)
                return NotFound();
            return View(seeker);
        }

        // GET: Delete
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var seeker = _jobSeekerRepo.GetById(id);
            if (seeker == null)
                return NotFound();
            return View(seeker);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(JobSeekerProfileDto dto)
        {
            try
            {
                _jobSeekerRepo.ChangeStatus(dto.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting JobSeeker profile");
                TempData["Error"] = "Error deleting record.";
                return RedirectToAction(nameof(Index));
            }
        }

        //  Upload CV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadCv(Guid id, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(cvFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Invalid file type. Only PDF, DOC, and DOCX are allowed.";
                    return RedirectToAction("Details", new { id });
                }

                var uploadDir = Path.Combine("wwwroot", "uploads", "cv");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    cvFile.CopyTo(stream);

                var seeker = _jobSeekerRepo.GetById(id);
                if (seeker == null)
                    return NotFound();

                seeker.CvFilePath = $"/uploads/cv/{uniqueFileName}";
                _jobSeekerRepo.Update(seeker);

                //  Success message
                TempData["Success"] = "✅ CV uploaded successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CV for Job Seeker {Id}", id);
                TempData["Error"] = "❌ An error occurred while uploading your CV.";
            }

            return RedirectToAction("Details", new { id });
        }

    }
}
