using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPISecurityCam.Data;
using RaspberryPISecurityCam.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using RaspberryPISecurityCam.Authorization;

namespace RaspberryPISecurityCam.Data
{
    public static class SeedData
    {
        #region Initialize
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using( var serviceScope = serviceProvider.CreateScope())
            using (var context = new ApplicationDbContext(
                serviceScope.ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // For sample purposes we are seeding 2 users both with the same password.
                // The password is set with the following command:
                // dotnet user-secrets set SeedUserPW <pw>
                // The admin user can do anything
                var adminID = await EnsureUser(serviceScope.ServiceProvider, testUserPw,"admin", "admin@admin.com",context);
                await EnsureRole(serviceScope.ServiceProvider, adminID, Constants.SecAppUserAdministratorsRole);

                SeedDB(context, adminID);
            }
        }
        #endregion

        #region CreateRoles        

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, 
                                                    string testUserPw,string userName, string email, ApplicationDbContext context)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser { UserName = userName, Email=email };
                await userManager.CreateAsync(user, testUserPw);
                var secAppUser = new SecAppUser() { Email = email, Name = userName, OwnerID = user.Id, Status = UserStatus.Submitted, isFirstLogin = true };
                context.Add(secAppUser);
                await context.SaveChangesAsync();
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                                      string uid, string role)
        {
            IdentityResult IR = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(uid);

            if(!(await userManager.IsInRoleAsync(user,role)))
                IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }        
        #endregion
        #region SeedDB
        public static void SeedDB(ApplicationDbContext context, string adminID)
        {
            if (context.SecAppUser.Any())
            {
                return;   // DB has been seeded
            }
        }
        #endregion
    }
}
