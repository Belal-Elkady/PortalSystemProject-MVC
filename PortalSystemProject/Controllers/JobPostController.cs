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
        public IActionResult Index(string search, Guid? categoryId, Guid? typeId, string sortOrder, int page = 1, int pageSize = 5)
        {
            var jobPosts = _jobPostRepo.GetAll();

            //  Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                jobPosts = jobPosts.Where(j =>
                    j.Title.ToLower().Contains(search) ||
                    (j.City != null && j.City.ToLower().Contains(search)) ||
                    (j.Country != null && j.Country.ToLower().Contains(search))
                ).ToList();
            }

            //  Filters
            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
                jobPosts = jobPosts.Where(j => j.JobCategoryId == categoryId.Value).ToList();

            if (typeId.HasValue && typeId.Value != Guid.Empty)
                jobPosts = jobPosts.Where(j => j.JobTypeId == typeId.Value).ToList();

            //  Sorting
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

            //  Pagination
            int totalItems = jobPosts.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            jobPosts = jobPosts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            //  Send view data
            ViewBag.JobCategories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", categoryId);
            ViewBag.JobTypes = new SelectList(_typeRepo.GetAll(), "Id", "Name", typeId);
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;

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
                //dto.CreatedByUserId = Guid.NewGuid(); // placeholder (later bind from logged-in user)
                dto.CreatedByUserId = new Guid("C593B4A8-7A98-4A9C-92D9-1018B41CDD72");

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
