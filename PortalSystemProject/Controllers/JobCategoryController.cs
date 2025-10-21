using BL.Contracts;
using BL.Dtos;
using Domains.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Controllers
{
    [Authorize(Roles = "Employer,Admin")]
    public class JobCategoryController : Controller
    {
        private readonly IJobCategoryRepository _jobCategoryRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JobCategoryController> _logger;

        public JobCategoryController(
            IJobCategoryRepository jobCategoryRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<JobCategoryController> logger)
        {
            _jobCategoryRepo = jobCategoryRepo;
            _userManager = userManager;
            _logger = logger;
        }

        // ✅ GET: JobCategory/Index
        public async Task<IActionResult> Index()
        {
            IEnumerable<JobCategoryDto> categories = new List<JobCategoryDto>();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var allCategories = _jobCategoryRepo.GetAll();

                if (User.IsInRole("Admin"))
                {
                    // Admin sees all
                    categories = allCategories;
                }
                else if (User.IsInRole("Employer"))
                {
                    // Employer sees their own categories
                    categories = allCategories
                        .Where(c => c.CreatedByUserId == currentUser.Id)
                        .ToList();
                }
                else
                {
                    // Others (Job Seekers) can see only active categories
                    categories = allCategories
                        .Where(c => c.IsActive)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job categories");
                TempData["Error"] = "❌ Failed to load job categories.";
                return View(new List<JobCategoryDto>());
            }

            return View(categories);
        }

        // ✅ GET: JobCategory/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ POST: JobCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                dto.CreatedAt = DateTime.UtcNow;
                dto.CreatedByUserId = currentUser.Id;

                _jobCategoryRepo.Add(dto);
                TempData["Success"] = "✅ Job category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating JobCategory");
                TempData["Error"] = "❌ Failed to create job category.";
                return View(dto);
            }
        }

        // ✅ GET: JobCategory/Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var category = _jobCategoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ POST: JobCategory/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, JobCategoryDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // only Admin or creator can edit
                if (!User.IsInRole("Admin"))
                {
                    var existing = _jobCategoryRepo.GetById(id);
                    if (existing == null || existing.CreatedByUserId != currentUser.Id)
                        return Forbid();
                }

                _jobCategoryRepo.Update(dto);
                TempData["Success"] = "✅ Job category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating JobCategory");
                TempData["Error"] = "❌ Failed to update job category.";
                return View(dto);
            }
        }

        // ✅ GET: JobCategory/Details/{id}
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var category = _jobCategoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ GET: JobCategory/Delete/{id}
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var category = _jobCategoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ POST: JobCategory/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _jobCategoryRepo.ChangeStatus(id);
                TempData["Success"] = "🗑️ Job category status changed successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting JobCategory");
                TempData["Error"] = "❌ Failed to delete job category.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
