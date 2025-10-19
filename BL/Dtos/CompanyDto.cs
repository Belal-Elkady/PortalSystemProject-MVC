using BL.Dtos.Base;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Dtos
{
    public class CompanyDto:BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? LogoUrl { get; set; }

        // ✅ Link company to the user who created it
        public Guid CreatedByUserId { get; set; }

        public string? Description { get; set; }
        // 1=Approved, 0=Pending
        public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
        public DateTime CreatedAt { get; set; }

        // Optional 
        public string? CreatedByUserName { get; set; }
        public string StatusText => Status.ToString();
    }
}
