using BL.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Dtos
{
    public class EmployerProfileDto:BaseDto
    {
        public Guid UserId { get; set; }         // FK -> ApplicationUser
        public Guid CompanyId { get; set; }      // FK -> Company
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? FullName { get; set; }
        public string? CompanyName { get; set; } //  Optional (for view use only)
    }
}
