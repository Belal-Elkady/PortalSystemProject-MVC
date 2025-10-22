using BL.Contracts;
using BL.Mapping;
using BL.Services;
using DAL.Contracts;
using DAL.DbContext;
using DAL.Repositories;
using Domains.UserModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace PortalSystemProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

           
           RegisterServciesHelper.RegisteredServices(builder);


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var config = services.GetRequiredService<IConfiguration>();

                // 1️- Ensure Roles Exist
                string[] roleNames = { "Admin", "Employer", "JobSeeker" };
                foreach (var roleName in roleNames)
                {
                    var roleExists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
                    if (!roleExists)
                    {
                        roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName }).GetAwaiter().GetResult();
                    }
                }

                // 2️- Read admin credentials from appsettings.json
                var adminSection = config.GetSection("AdminUser");
                var adminEmail = adminSection["Email"];
                var adminUserName = adminSection["UserName"];
                var adminPassword = adminSection["Password"];

                // 3️- Create admin if not exists
                if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
                {
                    var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                    if (adminUser == null)
                    {
                        adminUser = new ApplicationUser
                        {
                            UserName = adminUserName ?? adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true
                        };

                        var createResult = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
                        if (createResult.Succeeded)
                        {
                            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                            Console.WriteLine("✅ Admin user created successfully!");
                        }
                        else
                        {
                            Console.WriteLine("❌ Failed to create admin user:");
                            foreach (var err in createResult.Errors)
                                Console.WriteLine($" - {err.Description}");
                        }
                    }
                    else
                    {
                        // Ensure user has Admin role
                        var rolesForUser = userManager.GetRolesAsync(adminUser).GetAwaiter().GetResult();
                        if (!rolesForUser.Contains("Admin"))
                        {
                            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                        }
                    }
                }
            }
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Register}/{id?}");


            app.Run();
        }
    }
}
