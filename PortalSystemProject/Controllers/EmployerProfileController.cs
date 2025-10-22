using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace PortalSystemProject.Controllers
{
    [Authorize(Roles = "Employer,Admin,Employee")]
    public class EmployerProfileController : Controller
    {
        private readonly IEmployerProfileRepository _employerRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly ILogger<EmployerProfileController> _logger;

        public EmployerProfileController(
            IEmployerProfileRepository employerRepo,
            ICompanyRepository companyRepo,
            ILogger<EmployerProfileController> logger)
        {
            _employerRepo = employerRepo;
            _companyRepo = companyRepo;
            _logger = logger;
        }

        // Helper: Get current logged-in user ID
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // GET: EmployerProfile
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            var employers = _employerRepo.GetAll();

            // Only show this user's profiles
            var userProfiles = employers.Where(e => e.UserId == userId).ToList();
            return View(userProfiles);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EmployerProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                TempData["Error"] = "You must be logged in to create an employer profile.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Remove invalid model state for properties that will be set programmatically
            ModelState.Remove(nameof(dto.UserId));
            ModelState.Remove(nameof(dto.CreatedDate));
            ModelState.Remove(nameof(dto.CreatedBy));

            if (ModelState.IsValid)
            {
                // Set required properties
                dto.UserId = userId;
                dto.CreatedDate = DateTime.UtcNow; // ✅ Use CreatedDate, not CreatedAt
                dto.CreatedBy = userId;
                dto.Id = Guid.NewGuid(); // Generate new ID

                // Check for existing profile
                var existing = _employerRepo.GetAll().FirstOrDefault(e => e.UserId == userId);
                if (existing != null)
                {
                    TempData["Error"] = "You already have an employer profile.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _employerRepo.Add(dto);
                    TempData["Success"] = "✅ Employer profile created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error creating employer profile for user {UserId}", userId);
                    ModelState.AddModelError(string.Empty,
                        $"❌ Failed to create employer profile: {ex.Message}");
                }
            }
            else
            {
                // Log validation errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));
                ModelState.AddModelError(string.Empty, "❌ Some required fields are missing or invalid.");
            }

            // Reload dropdowns and return to form
            LoadDropdowns();
            return View(dto);
        }

        // GET: Edit
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var emp = _employerRepo.GetById(id);
            if (emp == null)
                return NotFound();

            // Security: Prevent editing other users' profiles
            if (emp.UserId != GetCurrentUserId())
                return Forbid();

            LoadDropdowns();
            return View(emp);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, EmployerProfileDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            // Remove validation for properties set by system
            ModelState.Remove(nameof(dto.UserId));
            ModelState.Remove(nameof(dto.UpdatedDate));
            ModelState.Remove(nameof(dto.UpdatedBy));

            if (ModelState.IsValid)
            {
                dto.UserId = userId;
                dto.UpdatedDate = DateTime.UtcNow;
                dto.UpdatedBy = userId;

                try
                {
                    _employerRepo.Update(dto);
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Employer Profile {Id}", id);
                    ModelState.AddModelError(string.Empty,
                        $"An error occurred while updating the profile: {ex.Message}");
                }
            }

            LoadDropdowns();
            return View(dto);
        }

        // GET: Details
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var emp = _employerRepo.GetById(id);
            if (emp == null)
                return NotFound();

            if (emp.UserId != GetCurrentUserId())
                return Forbid();

            return View(emp);
        }

        // GET: Delete
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var emp = _employerRepo.GetById(id);
            if (emp == null)
                return NotFound();

            if (emp.UserId != GetCurrentUserId())
                return Forbid();

            return View(emp);
        }

        // POST: Delete (Change status)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var emp = _employerRepo.GetById(id);
            if (emp == null)
                return NotFound();

            if (emp.UserId != GetCurrentUserId())
                return Forbid();

            try
            {
                _employerRepo.ChangeStatus(id);
                TempData["Success"] = "Profile deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Employer Profile {Id}", id);
                TempData["Error"] = "An error occurred while deleting the profile.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            var companies = _companyRepo.GetAll()
                .Where(c => c.CurrentState == 0) // Only active companies
                .ToList();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");
        }
    }
}