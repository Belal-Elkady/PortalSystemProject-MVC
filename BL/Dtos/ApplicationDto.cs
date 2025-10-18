using BL.Dtos.Base;
using System;

namespace BL.Dtos
{
    public class ApplicationDto : BaseDto
    {
        public Guid JobPostId { get; set; }       // FK -> JobPost
        public Guid JobSeekerId { get; set; }     // FK -> JobSeekerProfile
        public Guid ApplicantUserId { get; set; } // FK -> ApplicationUser

        public string? CoverLetter { get; set; }

        // Optional snapshot of CV at time of application
        public Guid? CVFileId { get; set; }       // FK -> uploaded CV file (optional)
        public string? CvFilePath { get; set; }   // For quick display/download

        // 0: Pending | 1: Reviewed | 2: Shortlisted | 3: Accepted | 4: Rejected | 5: Withdrawn
        public byte Status { get; set; } = 0;

        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        //  Optional display-only info (not stored)
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
    }
}
