using BL.Contracts;
using BL.Dtos;
using Domains.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PortalSystemProject.Controllers
{
    //[Authorize(Roles = "Admin,Employer")] // optional – enable later if roles are active
    public class JobTypeController : Controller
    {
        private readonly IJobTypeRepository _jobTypeRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JobTypeController> _logger;

        public JobTypeController(
            IJobTypeRepository jobTypeRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<JobTypeController> logger)
        {
            _jobTypeRepo = jobTypeRepo;
            _userManager = userManager;
            _logger = logger;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            IEnumerable<JobTypeDto> jobTypes = Enumerable.Empty<JobTypeDto>();

            try
            {
                var all = _jobTypeRepo.GetAll();

                // ✅ If not logged in (dev/test mode), show all
                if (User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    jobTypes = all;
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);

                    // Admins see all job types
                    if (User.IsInRole("Admin"))
                        jobTypes = all;
                    else
                        jobTypes = all.Where(j => j.CreatedByUserId == user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job types");
                TempData["Error"] = "❌ Failed to load job types.";
                return View(new List<JobTypeDto>());
            }

            return View(jobTypes);
        }

        // ================== CREATE ==================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobTypeDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var user = await _userManager.GetUserAsync(User);

                dto.CreatedByUserId = user != null ? user.Id : Guid.Empty;
                dto.CreatedAt = DateTime.UtcNow;

                _jobTypeRepo.Add(dto);
                TempData["Success"] = "✅ Job type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job type");
                TempData["Error"] = "❌ Failed to create job type.";
                return View(dto);
            }
        }

        // ================== EDIT ==================
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var jobType = _jobTypeRepo.GetById(id);
            if (jobType == null)
                return NotFound();

            return View(jobType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, JobTypeDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var existing = _jobTypeRepo.GetById(id);

                if (existing == null)
                    return NotFound();

                // Optional security check (only creator or admin can edit)
                if (!User.IsInRole("Admin") && existing.CreatedByUserId != user?.Id)
                    return Forbid();

                dto.CreatedByUserId = existing.CreatedByUserId;
                dto.CreatedAt = existing.CreatedAt;

                _jobTypeRepo.Update(dto);
                TempData["Success"] = "✅ Job type updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job type");
                TempData["Error"] = "❌ Failed to update job type.";
                return View(dto);
            }
        }

        // ================== DETAILS ==================
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var jobType = _jobTypeRepo.GetById(id);
            if (jobType == null)
                return NotFound();

            return View(jobType);
        }

        // ================== DELETE ==================
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var jobType = _jobTypeRepo.GetById(id);
            if (jobType == null)
                return NotFound();

            return View(jobType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _jobTypeRepo.ChangeStatus(id);
                TempData["Success"] = "🗑️ Job type status changed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job type");
                TempData["Error"] = "❌ Failed to delete job type.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
