using BL.Contracts;
using BL.Dtos;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PortalSystemProject.Controllers;
public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly ICompanyRepository _companyRepository;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IEmployerProfileRepository _employerProfile;
    public AccountController(IUserService userService,
        RoleManager<IdentityRole<Guid>> roleManager,
        IEmployerProfileRepository employerProfile,
        ICompanyRepository companyRepository)
    {
        _roleManager = roleManager;
        _userService = userService;
        _employerProfile = employerProfile;
        _companyRepository = companyRepository;
    }


    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterDto
        {
            Roles = _userService.GetRoles()
        };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            registerDto.Roles = _userService.GetRoles();
            return View(registerDto);
        }


        var result = await _userService.RegisterAsync(registerDto);

        if (!result.Success)
        {
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            registerDto.Roles = _userService.GetRoles();

            return View(registerDto);
        }


        if (result.Success)
        {
            var user = await _userService.GetUserByEmailAsync(registerDto.Email);

            if (user != null)
            {
                if (registerDto.Role == "Employer")
                {
                    return RedirectToAction("Create", "Company", new { userId = result.Id });
                }
                return RedirectToAction("Login", "Account");
            }
        }


        return View("Register", registerDto);
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto userDto)
    {
        if (!ModelState.IsValid)
            return View(userDto);

        var result = await _userService.LoginAsync(userDto);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(userDto);
        }

        // ✅ نجيب المستخدم من الداتابيز بعد تسجيل الدخول
        var dbUser = await _userService.GetUserByEmailAsync(userDto.Email);
        if (dbUser == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return View(userDto);
        }

        // ✅ لو المستخدم أدمن
        if (dbUser.Role != null && dbUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToRoute(new { area = "Admin", controller = "Admin", action = "Index" });
        }

        // ✅ لو المستخدم Employer
        if (dbUser.Role != null && dbUser.Role.Equals("Employer", StringComparison.OrdinalIgnoreCase))
        {
            // نجيب البروفايل الخاص بيه
            var employerProfile = _employerProfile
                .GetAll()
                .FirstOrDefault(e => e.UserId == dbUser.Id);

            // لو مفيش بروفايل أو مفيش شركة مرتبطة
            if (employerProfile == null || employerProfile.CompanyId == null)
            {
                TempData["Message"] = "You need to create your company profile first.";
                return RedirectToAction("CreateCompany", "Company");
            }

            // نجيب الشركة اللي تابعة له
            var company = _companyRepository.GetById(employerProfile.CompanyId);

            if (company == null)
            {
                TempData["Message"] = "Company not found. Please create it again.";
                return RedirectToAction("CreateCompany", "Company");
            }

            // ✅ نتحقق من حالة الشركة
            if (company.Status == CompanyStatus.Pending)
            {
                TempData["Message"] = "⏳ Your company is pending admin approval.";
                return RedirectToAction("PendingApproval", "Company");
            }

            if (company.Status == CompanyStatus.Rejected)
            {
                TempData["Message"] = "❌ Your company registration was rejected. Please contact support.";
                return RedirectToAction("CompanyRejected", "Company");
            }

            // ✅ لو الشركة approved يدخل عادي
            return RedirectToAction("EmployerDashboard", "Employer");
        }

        // ✅ باقي المستخدمين العاديين
        return RedirectToAction("AllJops", "Home");
    }
}
