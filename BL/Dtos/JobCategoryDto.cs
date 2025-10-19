using BL.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Dtos
{
    public class JobCategoryDto:BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // ✅ New fields (for user link)
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Optional - view only
        public string? CreatedByUserName { get; set; }
    }
}
