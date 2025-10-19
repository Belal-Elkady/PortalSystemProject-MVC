using BL.Contracts;
using BL.Dtos;
using Domains;
using Domains.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortalSystemProject.Controllers
{
    [Authorize] // ✅ Only logged-in users can access applications
    public class ApplicationController : Controller
    {
        private readonly IApplicationRepository _applicationRepo;
        private readonly IJobPostRepository _jobPostRepo;
        private readonly IJobSeekerProfileRepository _jobSeekerRepo;
        private readonly ILogger<ApplicationController> _logger;
        private readonly UserManager<ApplicationUser> _userManager; // ✅ for logged-in user

        public ApplicationController(
            IApplicationRepository applicationRepo,
            IJobPostRepository jobPostRepo,
            IJobSeekerProfileRepository jobSeekerRepo,
            ILogger<ApplicationController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _applicationRepo = applicationRepo;
            _jobPostRepo = jobPostRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _logger = logger;
            _userManager = userManager;
        }

        // ================== ADMIN/EMPLOYER VIEW ==================
        public IActionResult Index()
        {
            var apps = _applicationRepo.GetAll();
            return View(apps);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationDto dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "You must be logged in to apply.";
                    return RedirectToAction("Login", "Account");
                }

                dto.ApplicantUserId = user.Id;
                dto.AppliedAt = DateTime.Now;
                dto.Status = 0;

                _applicationRepo.Add(dto);
                TempData["Success"] = "Application created successfully.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(dto);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var app = _applicationRepo.GetById(id);
            if (app == null)
                return NotFound();

            LoadDropdowns();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ApplicationDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "You must be logged in to update applications.";
                    return RedirectToAction("Login", "Account");
                }

                if (dto.JobPostId == Guid.Empty || dto.JobSeekerId == Guid.Empty)
                {
                    ModelState.AddModelError("", "Job Post and Job Seeker are required.");
                    LoadDropdowns();
                    return View(dto);
                }

                if (dto.AppliedAt == default)
                    dto.AppliedAt = DateTime.Now;

                dto.ApplicantUserId = user.Id;

                try
                {
                    _applicationRepo.Update(dto);
                    TempData["Success"] = "Application updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Application");
                    ModelState.AddModelError("", "An error occurred while saving changes.");
                }
            }

            LoadDropdowns();
            return View(dto);
        }

        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var app = _applicationRepo.GetById(id);
            if (app == null)
                return NotFound();

            return View(app);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var app = _applicationRepo.GetById(id);
            if (app == null)
                return NotFound();

            return View(app);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _applicationRepo.ChangeStatus(id);
            TempData["Success"] = "Application deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ================== JOB SEEKER VIEW ==================

        [HttpGet]
        public async Task<IActionResult> Apply(Guid jobPostId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "You must be logged in to apply.";
                return RedirectToAction("Login", "Account");
            }

            var job = _jobPostRepo.GetById(jobPostId);
            if (job == null)
                return NotFound();

            var seeker = _jobSeekerRepo.GetAll().FirstOrDefault(s => s.UserId == user.Id);
            if (seeker == null)
            {
                TempData["Error"] = "Please create your Job Seeker profile first.";
                return RedirectToAction("Create", "JobSeekerProfile");
            }

            ViewBag.JobPostTitle = job.Title;

            var dto = new ApplicationDto
            {
                JobPostId = jobPostId,
                JobSeekerId = seeker.Id
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplicationDto dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "You must be logged in to apply.";
                    return RedirectToAction("Login", "Account");
                }

                dto.ApplicantUserId = user.Id;
                dto.AppliedAt = DateTime.Now;
                dto.Status = 0;

                _applicationRepo.Add(dto);
                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction(nameof(MyApplications));
            }

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "You must be logged in to view your applications.";
                return RedirectToAction("Login", "Account");
            }

            var seeker = _jobSeekerRepo.GetAll().FirstOrDefault(s => s.UserId == user.Id);
            if (seeker == null)
            {
                TempData["Error"] = "Please create your Job Seeker profile first.";
                return RedirectToAction("Create", "JobSeekerProfile");
            }

            var apps = _applicationRepo.GetAll()
                .Where(a => a.JobSeekerId == seeker.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToList();

            return View(apps);
        }

        private void LoadDropdowns()
        {
            ViewBag.JobPosts = new SelectList(_jobPostRepo.GetAll(), "Id", "Title");
            ViewBag.JobSeekers = new SelectList(_jobSeekerRepo.GetAll(), "Id", "Headline");
        }
    }
}
