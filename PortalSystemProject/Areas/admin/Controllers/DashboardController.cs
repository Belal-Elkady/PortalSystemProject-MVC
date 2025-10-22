using BL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICompanyRepository _companyRepo;
        private readonly IJobPostRepository _jobRepo;
        private readonly IApplicationRepository _applicationRepo;

        public DashboardController(
            IUserService userService,
            ICompanyRepository companyRepo,
            IJobPostRepository jobRepo,
            IApplicationRepository applicationRepo)
        {
            _userService = userService;
            _companyRepo = companyRepo;
            _jobRepo = jobRepo;
            _applicationRepo = applicationRepo;
        }

        public async Task<IActionResult> Index()
        {
            // ✅ إحصائيات عامة للوحة التحكم
            var users = await _userService.GetAllUsersAsync();
            var companies = _companyRepo.GetAll();
            var jobs = _jobRepo.GetAll();
            var applications = _applicationRepo.GetAll();

            ViewBag.TotalUsers = users.Count();
            ViewBag.TotalCompanies = companies.Count();
            ViewBag.TotalJobs = jobs.Count();
            ViewBag.TotalApplications = applications.Count();

            return View();
        }
    }
}
