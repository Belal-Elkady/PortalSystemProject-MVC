using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepo;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ICompanyRepository companyRepo, ILogger<CompanyController> logger)
        {
            _companyRepo = companyRepo;
            _logger = logger;
        }

        // GET: Company/Index
        public IActionResult Index()
        {
            var companies = _companyRepo.GetAll();
            return View(companies);
        }

        // GET: Company/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyDto dto)
        {
            if (ModelState.IsValid)
            {
                _companyRepo.Add(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Company/Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var company = _companyRepo.GetById(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        // POST: Company/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, CompanyDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _companyRepo.Update(dto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating company");
                    throw;
                }
            }
            return View(dto);
        }

        // GET: Company/Details/{id}
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var company = _companyRepo.GetById(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        // GET: Company/Delete/{id}
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var company = _companyRepo.GetById(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        // POST: Company/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _companyRepo.ChangeStatus(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company");
                throw;
            }
        }
    }
}
