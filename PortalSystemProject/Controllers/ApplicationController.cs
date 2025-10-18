using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace PortalSystemProject.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly IApplicationRepository _applicationRepo;
        private readonly IJobPostRepository _jobPostRepo;
        private readonly IJobSeekerProfileRepository _jobSeekerRepo;
        private readonly ILogger<ApplicationController> _logger;

        private readonly Guid _testUserId = new Guid("C593B4A8-7A98-4A9C-92D9-1018B41CDD72");

        public ApplicationController(
            IApplicationRepository applicationRepo,
            IJobPostRepository jobPostRepo,
            IJobSeekerProfileRepository jobSeekerRepo,
            ILogger<ApplicationController> logger)
        {
            _applicationRepo = applicationRepo;
            _jobPostRepo = jobPostRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _logger = logger;
        }

        // ================== BASIC CRUD ==================

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
        public IActionResult Create(ApplicationDto dto)
        {
            if (ModelState.IsValid)
            {
                dto.ApplicantUserId = _testUserId;
                dto.AppliedAt = DateTime.Now;
                dto.Status = 0;

                _applicationRepo.Add(dto);
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
        public IActionResult Edit(Guid id, ApplicationDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                // 🩵 Ensure mandatory values are set
                if (dto.JobPostId == Guid.Empty || dto.JobSeekerId == Guid.Empty)
                {
                    ModelState.AddModelError("", "Job Post and Job Seeker are required.");
                    LoadDropdowns(); // re-fill your SelectLists
                    return View(dto);
                }

                // 🩵 Ensure AppliedAt is valid
                if (dto.AppliedAt == default)
                    dto.AppliedAt = DateTime.Now;

                // 🩵 Ensure ApplicantUserId placeholder exists
                if (dto.ApplicantUserId == Guid.Empty)
                    dto.ApplicantUserId = new Guid("C593B4A8-7A98-4A9C-92D9-1018B41CDD72"); // test user

                try
                {
                    _applicationRepo.Update(dto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Application");
                    ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
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
            return RedirectToAction(nameof(Index));
        }

        // ================== JOB SEEKER METHODS ==================

        // Apply to a specific job (from JobPost list)
        [HttpGet]
        public IActionResult Apply(Guid jobPostId)
        {
            var job = _jobPostRepo.GetById(jobPostId);
            if (job == null)
                return NotFound();

            ViewBag.JobPostTitle = job.Title;

            // Select job seeker (temporary test user)
            var seeker = _jobSeekerRepo.GetAll()
                .FirstOrDefault(x => x.UserId == _testUserId);

            if (seeker == null)
            {
                TempData["Error"] = "No Job Seeker profile found for this user.";
                return RedirectToAction("Index", "JobSeekerProfile");
            }

            var dto = new ApplicationDto
            {
                JobPostId = jobPostId,
                JobSeekerId = seeker.Id
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(ApplicationDto dto)
        {
            if (ModelState.IsValid)
            {
                dto.ApplicantUserId = _testUserId;
                dto.AppliedAt = DateTime.Now;
                dto.Status = 0; // Pending

                _applicationRepo.Add(dto);
                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction(nameof(MyApplications));
            }

            return View(dto);
        }

        // List applications submitted by the current (test) user
        public IActionResult MyApplications()
        {
            var seeker = _jobSeekerRepo.GetAll()
                .FirstOrDefault(x => x.UserId == _testUserId);

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
