using BL.Contracts;
using BL.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Controllers
{
    public class JobTypeController : Controller
    {
        private readonly IJobTypeRepository _jobTypeRepo;

        public JobTypeController(IJobTypeRepository jobTypeRepo)
        {
            _jobTypeRepo = jobTypeRepo;
        }

        // GET: /JobType
        public IActionResult Index()
        {
            var jobTypes = _jobTypeRepo.GetAll();
            return View(jobTypes);
        }

        // GET: /JobType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /JobType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JobTypeDto dto)
        {
            if (ModelState.IsValid)
            {
                _jobTypeRepo.Add(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: /JobType/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            var jobType = _jobTypeRepo.GetById(id);
            if (jobType == null)
                return NotFound();

            return View(jobType);
        }

        // POST: /JobType/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, JobTypeDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                _jobTypeRepo.Update(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: /JobType/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            var jobType = _jobTypeRepo.GetById(id);
            if (jobType == null)
                return NotFound();

            return View(jobType);
        }

        // POST: /JobType/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _jobTypeRepo.ChangeStatus(id, 1); // or use Delete() if you prefer
            return RedirectToAction(nameof(Index));
        }

        // GET: /JobType/Details/{id}
        public IActionResult Details(Guid id)
        {
            var jobType = _jobTypeRepo.GetById(id);
            if (jobType == null)
                return NotFound();

            return View(jobType);
        }
    }
}
