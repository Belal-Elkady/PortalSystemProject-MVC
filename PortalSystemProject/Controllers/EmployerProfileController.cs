using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace PortalSystemProject.Controllers
{
    [Authorize(Roles = "Employer,Admin")]
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

        //  Helper: Get current logged-in user ID
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

            //  Only show this user's profiles
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

    if (ModelState.IsValid)
    {
        dto.UserId = userId;
        dto.CreatedAt = DateTime.UtcNow; // ✅ important

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
            _logger.LogError(ex, "❌ Error creating employer profile");
            // ❌ Error message shown when staying on the same form
            ModelState.AddModelError(string.Empty, "❌ Failed to create employer profile. Please check your input and try again.");
        }
    }
    else
    {
        // Validation errors
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

            // 🧩 Security: Prevent editing other users' profiles
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

            if (ModelState.IsValid)
            {
                dto.UserId = userId;

                try
                {
                    _employerRepo.Update(dto);
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Employer Profile");
                    TempData["Error"] = "An error occurred while updating the profile.";
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

            _employerRepo.ChangeStatus(id);
            TempData["Success"] = "Profile deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Companies = new SelectList(_companyRepo.GetAll(), "Id", "Name");
        }
    }
}
