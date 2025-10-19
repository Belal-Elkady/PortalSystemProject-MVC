using BL.Dtos.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Dtos;

public class RegisterDto : BaseDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "User name is required")]
    [StringLength(50, ErrorMessage = " Name cannot exceed 50 characters")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^(\+?\d{1,3}[- ]?)?\d{9,12}$", ErrorMessage = "Please enter a valid phone number")]
    public string Phone { get; set; }

    public string? Role { get; set; }

    public string? ReturnUrl { get; set; }

    public List<string> Roles { get; set; } = new List<string>();
}