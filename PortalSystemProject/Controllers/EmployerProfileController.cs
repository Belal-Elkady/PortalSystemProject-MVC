using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortalSystemProject.Controllers
{
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

        // GET: EmployerProfile
        public IActionResult Index()
        {
            var employers = _employerRepo.GetAll();
            return View(employers);
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
            if (ModelState.IsValid)
            {
                // 🧩 Temporary user ID (replace later when auth is added)
                dto.UserId = new Guid("C593B4A8-7A98-4A9C-92D9-1018B41CDD72");

                _employerRepo.Add(dto);
                return RedirectToAction(nameof(Index));
            }

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

            if (ModelState.IsValid)
            {
                _employerRepo.Update(dto);
                return RedirectToAction(nameof(Index));
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

            return View(emp);
        }

        // GET: Delete
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var emp = _employerRepo.GetById(id);
            if (emp == null)
                return NotFound();

            return View(emp);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _employerRepo.ChangeStatus(id);
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Companies = new SelectList(_companyRepo.GetAll(), "Id", "Name");
        }
    }
}
