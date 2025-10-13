using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Controllers
{
    public class JobCategoryController : Controller
    {
        private readonly IJobCategoryRepository _jobCategoryRepo;
        private readonly ILogger<JobCategoryController> _logger;

        public JobCategoryController(IJobCategoryRepository jobCategoryRepo, ILogger<JobCategoryController> logger)
        {
            _jobCategoryRepo = jobCategoryRepo;
            _logger = logger;
        }

        // GET: JobCategory/Index
        public IActionResult Index()
        {
            var categories = _jobCategoryRepo.GetAll();
            return View(categories);
        }

        // GET: JobCategory/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: JobCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JobCategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                _jobCategoryRepo.Add(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: JobCategory/Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var category = _jobCategoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: JobCategory/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, JobCategoryDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _jobCategoryRepo.Update(dto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating JobCategory");
                    throw;
                }
            }
            return View(dto);
        }

        // GET: JobCategory/Details/{id}
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var category = _jobCategoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: JobCategory/Delete/{id}
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var category = _jobCategoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: JobCategory/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _jobCategoryRepo.ChangeStatus(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting JobCategory");
                throw;
            }
        }
    }
}
