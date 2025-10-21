using BL.Contracts;
using BL.Dtos;
using Domains.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortalSystemProject.Controllers
{
    [Authorize]
    public class JobPostController : Controller
    {
        private readonly IJobPostRepository _jobPostRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly IJobCategoryRepository _categoryRepo;
        private readonly IJobTypeRepository _typeRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JobPostController> _logger;

        public JobPostController(
            IJobPostRepository jobPostRepo,
            ICompanyRepository companyRepo,
            IJobCategoryRepository categoryRepo,
            IJobTypeRepository typeRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<JobPostController> logger)
        {
            _jobPostRepo = jobPostRepo;
            _companyRepo = companyRepo;
            _categoryRepo = categoryRepo;
            _typeRepo = typeRepo;
            _userManager = userManager;
            _logger = logger;
        }

        // =====================================================
        // 🔹 INDEX — Browse & filter jobs by category, type, location
        // =====================================================
        public async Task<IActionResult> Index(
            string search,
            Guid? categoryId,
            Guid? typeId,
            string sortOrder,
            int page = 1,
            int pageSize = 5)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var jobPosts = _jobPostRepo.GetAll();

            // 🧭 Role-based view
            if (User.IsInRole("Employer"))
            {
                jobPosts = jobPosts.Where(j => j.CreatedByUserId == currentUser.Id).ToList();
            }
            else if (User.IsInRole("JobSeeker"))
            {
                jobPosts = jobPosts.Where(j => j.IsActive).ToList();
            }

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                jobPosts = jobPosts.Where(j =>
                    j.Title.ToLower().Contains(search) ||
                    (j.City != null && j.City.ToLower().Contains(search)) ||
                    (j.Country != null && j.Country.ToLower().Contains(search))
                ).ToList();
            }

            // 🎯 Filters
            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
                jobPosts = jobPosts.Where(j => j.JobCategoryId == categoryId.Value).ToList();

            if (typeId.HasValue && typeId.Value != Guid.Empty)
                jobPosts = jobPosts.Where(j => j.JobTypeId == typeId.Value).ToList();

            // 🧩 Sorting
            ViewBag.TitleSort = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewBag.CitySort = sortOrder == "city_asc" ? "city_desc" : "city_asc";
            ViewBag.SalarySort = sortOrder == "salary_asc" ? "salary_desc" : "salary_asc";
            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            jobPosts = sortOrder switch
            {
                "title_desc" => jobPosts.OrderByDescending(j => j.Title).ToList(),
                "title_asc" => jobPosts.OrderBy(j => j.Title).ToList(),
                "city_desc" => jobPosts.OrderByDescending(j => j.City).ToList(),
                "city_asc" => jobPosts.OrderBy(j => j.City).ToList(),
                "salary_desc" => jobPosts.OrderByDescending(j => j.MaxSalary).ToList(),
                "salary_asc" => jobPosts.OrderBy(j => j.MinSalary).ToList(),
                "date_desc" => jobPosts.OrderByDescending(j => j.PublishedAt).ToList(),
                "date_asc" => jobPosts.OrderBy(j => j.PublishedAt).ToList(),
                _ => jobPosts.OrderByDescending(j => j.PublishedAt).ToList()
            };

            // 📄 Pagination
            int totalItems = jobPosts.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            jobPosts = jobPosts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 🧾 Send data to View
            ViewBag.JobCategories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", categoryId);
            ViewBag.JobTypes = new SelectList(_typeRepo.GetAll(), "Id", "Name", typeId);
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;

            return View(jobPosts);
        }

        // =====================================================
        // 🔹 CREATE
        // =====================================================
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostDto dto)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(dto);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                dto.PublishedAt = DateTime.UtcNow;
                dto.CreatedByUserId = currentUser.Id;
                dto.IsActive = true;

                _jobPostRepo.Add(dto);
                TempData["Success"] = "✅ Job post created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job post");
                TempData["Error"] = "❌ Failed to create job post.";
                LoadDropdowns();
                return View(dto);
            }
        }

        // =====================================================
        // 🔹 EDIT
        // =====================================================
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var jobPost = _jobPostRepo.GetById(id);
            if (jobPost == null)
                return NotFound();

            LoadDropdowns();
            return View(jobPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, JobPostDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(dto);
            }

            try
            {
                var existing = _jobPostRepo.GetById(id);
                if (existing == null)
                    return NotFound();

                // Preserve existing fields
                dto.PublishedAt = existing.PublishedAt;
                dto.CreatedByUserId = existing.CreatedByUserId;
                dto.IsActive = existing.IsActive;

                _jobPostRepo.Update(dto);
                TempData["Success"] = "✅ Job post updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job post");
                TempData["Error"] = "❌ Failed to update job post.";
                LoadDropdowns();
                return View(dto);
            }
        }

        // =====================================================
        // 🔹 DETAILS
        // =====================================================
        public IActionResult Details(Guid id)
        {
            var jobPost = _jobPostRepo.GetById(id);
            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        // =====================================================
        // 🔹 DELETE (ChangeStatus)
        // =====================================================
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var jobPost = _jobPostRepo.GetById(id);
            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _jobPostRepo.ChangeStatus(id);
                TempData["Success"] = "🗑️ Job post deleted (deactivated) successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job post");
                TempData["Error"] = "❌ Failed to delete job post.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // 🔹 Helper to load dropdowns
        // =====================================================
        private void LoadDropdowns()
        {
            ViewBag.Companies = new SelectList(_companyRepo.GetAll(), "Id", "Name");
            ViewBag.JobCategories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            ViewBag.JobTypes = new SelectList(_typeRepo.GetAll(), "Id", "Name");
        }
    }
}
