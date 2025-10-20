using BL.Contracts;
using BL.Dtos;
using Domains.UserModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace PortalSystemProject.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IHttpContextAccessor contextAccessor
        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _contextAccessor = contextAccessor;

    }


    //public async Task<UserResultDto> RegisterAsync(RegisterDto registerDto)
    //{
    //    // ✅ Check if phone number already exists
    //    var existingUser = await _userManager.Users
    //        .FirstOrDefaultAsync(u => u.PhoneNumber == registerDto.Phone);

    //    if (existingUser != null)
    //    {
    //        return new UserResultDto
    //        {
    //            Success = false,
    //            Errors = new List<string> { "This phone number is already registered." }
    //        };
    //    }

    //    // ✅ Check if email already exists (optional but recommended)
    //    var existingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
    //    if (existingEmail != null)
    //    {
    //        return new UserResultDto
    //        {
    //            Success = false,
    //            Errors = new List<string> { "This email is already registered." }
    //        };
    //    }

    //    // ✅ Create user if not duplicate
    //    var user = new ApplicationUser
    //    {
    //        UserName = registerDto.UserName,
    //        Email = registerDto.Email,
    //        PhoneNumber = registerDto.Phone
    //    };

    //    var result = await _userManager.CreateAsync(user, registerDto.Password);

    //    if (!result.Succeeded)
    //    {
    //        return new UserResultDto
    //        {
    //            Success = false,
    //            Errors = result.Errors?.Select(e => e.Description)
    //        };
    //    }

    //    var roleName = string.IsNullOrEmpty(registerDto.Role) ? "User" : registerDto.Role;
    //    await _userManager.AddToRoleAsync(user, roleName);

    //    var appUser = await _userManager.FindByEmailAsync(registerDto.Email);
    //    await _signInManager.SignInAsync(appUser, isPersistent: false);

    //    return new UserResultDto
    //    {
    //        Success = true,
    //        Id = user.Id
    //    };
    //}



    public async Task<UserResultDto> RegisterAsync(RegisterDto registerDto)
    {
        // تحقق إن الإيميل مش موجود
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new UserResultDto
            {
                Success = false,
                Errors = new[] { "This email is already registered." }
            };
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.Phone
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new UserResultDto
            {
                Success = false,
                Errors = result.Errors?.Select(e => e.Description)
            };
        }

        // add the role
        var roleName = string.IsNullOrEmpty(registerDto.Role) ? "JobSeeker" : registerDto.Role;

        // sure the role exists
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
        }

        // add user to the new role
        await _userManager.AddToRoleAsync(user, roleName);

        //login the user after register
        var appUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (appUser != null)
        {
            // نجهز Claims فيها اسم المستخدم بدل الإيميل
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
        new Claim(ClaimTypes.Name, appUser.UserName ?? appUser.Email), // 👈 هنا الاسم اللي هيظهر فوق
        new Claim(ClaimTypes.Email, appUser.Email)
    };

            // نضيف الـ claims دي للمستخدم
            await _userManager.AddClaimsAsync(appUser, claims);

            // ونسجل دخوله بالـ claims دي
            await _signInManager.SignInWithClaimsAsync(appUser, isPersistent: false, claims);
        }

        return new UserResultDto
        {
            Success = true,
            Id = user.Id,
            // من المفيد إرجاع الدور عشان الـ Controller يتصرف بناءً عليه
            Errors = null
        };
    }

    public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, false);

        if (!result.Succeeded)
        {
            return new UserResultDto
            {
                Success = false,
                Errors = new[] { "Invalid login attempt." }
            };
        }

        // Generate token (if needed) or return success
        return new UserResultDto { Success = true, Token = "DummyTokenForNow" };

    }
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
    public async Task<RegisterDto> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new RegisterDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Phone = user.PhoneNumber,
            Role = roles.FirstOrDefault()
        };
    }

    public async Task<RegisterDto> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new RegisterDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Phone = user.PhoneNumber,
            Role = roles.FirstOrDefault()
        };
    }

    public async Task<IEnumerable<RegisterDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users
        .Where(u => u.UserName != "admin@gmail.com")
        .ToList();

        var result = new List<RegisterDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user); // 👈 نجيب الـ roles

            result.Add(new RegisterDto
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.PhoneNumber,
                UserName = user.UserName,
                Role = roles.FirstOrDefault() ?? "No Role" // 👈 أول Role أو "No Role"
            });
        }
        return result;
    }

    public async Task<UserResultDto> AddRoleAsync(RoleDto roleDto)
    {

        if (roleDto == null || string.IsNullOrWhiteSpace(roleDto.RoleName))
        {
            return new UserResultDto
            {
                Success = false,
                Errors = new[] { "Role name is required." }
            };
        }

        var role = new IdentityRole<Guid>
        {
            Name = roleDto.RoleName.Trim()
        };

        var result = await _roleManager.CreateAsync(role);

        return new UserResultDto
        {
            Success = result.Succeeded,
            Errors = result.Errors?.Select(e => e.Description)
        };
    }
    public Guid GetLoggedInUser()
    {
        var userId = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId);
    }

    public List<string> GetRoles()
    {
        return _roleManager.Roles
                 .Where(r => r.Name != "Admin")
                 .Select(r => r.Name)
                 .ToList();

    }
}
