using BL.Contracts;
using BL.Dtos;
using Domains;
using Domains.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Controllers
{
    [Authorize(Roles = "Employer,Admin")] // Only employers or admins can manage companies
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepo;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(
            ICompanyRepository companyRepo,
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            ILogger<CompanyController> logger)
        {
            _companyRepo = companyRepo;
            _userService = userService;
            _userManager = userManager;
            _logger = logger;
        }

        //  List all companies (Admin can view all, Employer can view only their own)
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            IEnumerable<CompanyDto> companies = Enumerable.Empty<CompanyDto>();

            try
            {
                var allCompanies = _companyRepo.GetAll();

                if (User.IsInRole("Admin"))
                {
                    // ✅ Admins see all companies
                    companies = allCompanies;
                }
                else if (User.IsInRole("Employer"))
                {
                    // ✅ Employers see only their own companies (excluding rejected)
                    companies = allCompanies
                        .Where(c => c.CreatedByUserId == currentUser.Id &&
                                    c.Status != Domains.CompanyStatus.Rejected)
                        .ToList();
                }
                else
                {
                    // ✅ Other users (like JobSeekers) see only Approved companies
                    companies = allCompanies
                        .Where(c => c.Status == Domains.CompanyStatus.Approved)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading companies");
                TempData["Error"] = "❌ Failed to load company list.";
                return View(new List<CompanyDto>());
            }

            return View(companies);
        }


        //  GET: Company/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //  POST: Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var currentUser = await _userManager.GetUserAsync(User);

            dto.CreatedAt = DateTime.UtcNow;
            dto.Status = CompanyStatus.Pending; // default new companies pending approval
            dto.CreatedByUserId = currentUser.Id; // link company to the logged-in employer

            try
            {
                _companyRepo.Add(dto);
                TempData["Success"] = "✅ Company created successfully and is pending admin approval.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                TempData["Error"] = "❌ Failed to create company. Please try again.";
                return View(dto);
            }
        }

        //  GET: Company/Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var company = _companyRepo.GetById(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        //  POST: Company/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CompanyDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            var currentUser = await _userManager.GetUserAsync(User);

            // Optional: ensure user can only edit their own company (if not Admin)
            if (!User.IsInRole("Admin"))
            {
                var existing = _companyRepo.GetById(id);
                if (existing == null || existing.CreatedByUserId != currentUser.Id)
                    return Forbid();
            }

            try
            {
                _companyRepo.Update(dto);
                TempData["Success"] = "✅ Company updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company");
                TempData["Error"] = "❌ Failed to update company. Please try again.";
                return View(dto);
            }
        }

        //  GET: Company/Details/{id}
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var company = _companyRepo.GetById(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        //  GET: Company/Delete/{id}
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var company = _companyRepo.GetById(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        //  POST: Company/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _companyRepo.ChangeStatus(id);
                TempData["Success"] = "🗑️ Company status changed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company");
                TempData["Error"] = "❌ Failed to delete company.";
                return RedirectToAction(nameof(Index));
            }
        }

        //  Custom status views (optional for UX)
        [HttpGet]
        public IActionResult PendingApproval()
        {
            ViewBag.Message = TempData["Message"] ?? "⏳ Your company is pending admin approval.";
            return View("StatusMessage");
        }

        [HttpGet]
        public IActionResult CompanyRejected()
        {
            ViewBag.Message = TempData["Message"] ?? "❌ Your company registration was rejected.";
            return View("StatusMessage");
        }
    }
}
