using AutoMapper;
using BL.Dtos;
using Domains;
using Domains.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Mapping
{
    public class MappingProfile :Profile
    {
        public MappingProfile()
        {
            //CreateMap<TestClass, TestDto>().ReverseMap();


            // CreateMap<TestClass, TestDto>()
            //.ForMember(dest => dest.FullName,
            //    opt => opt.MapFrom(src => $"{src.FName} {src.LName}"))
            //.ReverseMap();

            CreateMap<Application, ApplicationDto>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<JobCategory, JobCategoryDto>().ReverseMap();
            CreateMap<JobPost, JobPostDto>().ReverseMap();
            CreateMap<JobType, JobTypeDto>().ReverseMap();
            CreateMap<CVFile, CVFileDto>().ReverseMap();
            CreateMap<JobSeekerProfile, JobSeekerProfileDto>().ReverseMap();
            CreateMap<SavedJob, SavedJobDto>().ReverseMap();
            CreateMap<EmployerProfile, EmployerProfileDto>().ReverseMap();




            CreateMap<JobPost, JobPostDto>()
           .ForMember(dest => dest.CompanyName,
                      opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
           .ForMember(dest => dest.JobCategoryName,
                      opt => opt.MapFrom(src => src.JobCategory != null ? src.JobCategory.Name : null))
           .ForMember(dest => dest.JobTypeName,
                      opt => opt.MapFrom(src => src.JobType != null ? src.JobType.Name : null))
           .ForMember(dest => dest.PublishedAt,
                      opt => opt.MapFrom(src => src.CreatedDate ?? DateTime.MinValue)) // adjust if you store publishedAt separately
           .ForMember(dest => dest.CreatedByUserName,
                      opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.UserName : null));

            // JobPostDto -> JobPost
            CreateMap<JobPostDto, JobPost>()
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.JobCategory, opt => opt.Ignore())
                .ForMember(dest => dest.JobType, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                // if your entity uses CreatedDate/CreatedAt fields, map appropriately, or ignore to preserve existing values
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Condition((src, dest, srcMember) => srcMember != Guid.Empty));


        }
    }
}
