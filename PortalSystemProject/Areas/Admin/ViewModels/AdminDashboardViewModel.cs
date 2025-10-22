using System;
using System.Collections.Generic;

namespace PortalSystemProject.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }

        public List<CompanyInfo> RecentCompanies { get; set; }
        public List<JobInfo> RecentJobs { get; set; }
    }

    public class CompanyInfo
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class JobInfo
    {
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
