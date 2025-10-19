using BL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts;
public interface IUserService
{
    Task<UserResultDto> RegisterAsync(RegisterDto registerDto);
    Task<UserResultDto> LoginAsync(LoginDto loginDto);
    Task LogoutAsync();
    Task<RegisterDto> GetUserByIdAsync(string userId);
    Task<RegisterDto> GetUserByEmailAsync(string email);
    Task<IEnumerable<RegisterDto>> GetAllUsersAsync();
    Task<UserResultDto> AddRoleAsync(RoleDto roleDto);
    Guid GetLoggedInUser();

    List<string> GetRoles();
}