using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortalSystemProject.Controllers
{
    public class JobPostController : Controller
    {
        private readonly IJobPostRepository _jobPostRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly IJobCategoryRepository _categoryRepo;
        private readonly IJobTypeRepository _typeRepo;
        private readonly ILogger<JobPostController> _logger;

        public JobPostController(
            IJobPostRepository jobPostRepo,
            ICompanyRepository companyRepo,
            IJobCategoryRepository categoryRepo,
            IJobTypeRepository typeRepo,
            ILogger<JobPostController> logger)
        {
            _jobPostRepo = jobPostRepo;
            _companyRepo = companyRepo;
            _categoryRepo = categoryRepo;
            _typeRepo = typeRepo;
            _logger = logger;
        }

        // GET: JobPost
        public IActionResult Index()
        {
            var jobPosts = _jobPostRepo.GetAll();
            return View(jobPosts);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

       //POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JobPostDto dto)
        {
            if (ModelState.IsValid)
            {
                dto.PublishedAt = DateTime.Now;
                dto.CreatedByUserId = Guid.NewGuid(); // placeholder (later bind from logged-in user)
                _jobPostRepo.Add(dto);
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(dto);
        }
        // POST: Create rtying to work it with out FK user id but it failed
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(JobPostDto dto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        dto.PublishedAt = DateTime.Now;

        //        // TEMP FIX: comment this line (it causes FK issue)
        //        // dto.CreatedByUserId = Guid.NewGuid();

        //        //  instead, use a placeholder existing user or Guid.Empty
        //        dto.CreatedByUserId = Guid.Empty;

        //        _jobPostRepo.Add(dto);
        //        return RedirectToAction(nameof(Index));
        //    }

        //    LoadDropdowns();
        //    return View(dto);
        //}


        // GET: Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var jobPost = _jobPostRepo.GetById(id);
            if (jobPost == null)
                return NotFound();

            LoadDropdowns();
            return View(jobPost);
        }

        // POST: Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, JobPostDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _jobPostRepo.Update(dto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating JobPost");
                    throw;
                }
            }

            LoadDropdowns();
            return View(dto);
        }

        // GET: Details/{id}
        public IActionResult Details(Guid id)
        {
            var jobPost = _jobPostRepo.GetById(id);
            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        // GET: Delete/{id}
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var jobPost = _jobPostRepo.GetById(id);
            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        // POST: Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _jobPostRepo.ChangeStatus(id);
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Companies = new SelectList(_companyRepo.GetAll(), "Id", "Name");
            ViewBag.JobCategories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            ViewBag.JobTypes = new SelectList(_typeRepo.GetAll(), "Id", "Name");
        }
    }
}
