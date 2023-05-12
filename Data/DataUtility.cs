using CrucibleBlog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CrucibleBlog.Data
{
    public static class DataUtility //static class is a class that gets created (instantiated) at the time you define it.
    {
        private const string? _adminRole = "Admin";
        private const string? _moderatorRole = "Moderator";
        public static string GetConnectionString(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
            //if (string.IsNullOrEmpty(databaseUrl))
            //{
            //    return connectionString;
            //}
            //else
            //{
            //    return BuildConnectionString(databaseUrl);
            //}
        }
        private static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            // obtaining the necessary services based on the IServiceProvider parameter
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();

            // align the db by checking migrations
            await dbContextSvc.Database.MigrateAsync();

            //Seed Application roles
            await SeedRolesAsync(roleManagerSvc);

            //seed demo user(s)
            await SeedBlogUsersAsync(userManagerSvc, configurationSvc);

        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(_adminRole!)) //check to make sure role does NOT(! at beginning) exist
            {
                await roleManager.CreateAsync(new IdentityRole(_adminRole!)); //create new Admin role
            }
            if (!await roleManager.RoleExistsAsync(_moderatorRole!)) //check to make sure role does NOT(! at beginning) exist
            {
                await roleManager.CreateAsync(new IdentityRole(_moderatorRole!)); //create new mod role
            }
        }


        private static async Task SeedBlogUsersAsync(UserManager<BlogUser> userManager, IConfiguration configuration)
        {
            string? adminEmail = configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
            string? adminPassword = configuration["AdminLoginPassword"] ?? Environment.GetEnvironmentVariable("AdminLoginPassword");

            string? moderatorEmail = configuration["ModeratorLoginEmail"] ?? Environment.GetEnvironmentVariable("ModeratorLoginEmail");
            string? moderatorPassword = configuration["ModeratorLoginPassword"] ?? Environment.GetEnvironmentVariable("ModeratorLoginPassword");

            try
            {
                //Seed Admin
                BlogUser? adminUser = new BlogUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Khala",
                    LastName = "Wright",
                    EmailConfirmed = true
                };
                BlogUser? user = await userManager.FindByEmailAsync(adminUser.Email!);

                if (user == null)
                {
                    await userManager.CreateAsync(adminUser, adminPassword!);
                    await userManager.AddToRoleAsync(adminUser, _adminRole!); //add admin role to adminUser

                }

                //Seed Moderator
                BlogUser? moderatorUser = new BlogUser()
                {
                    UserName = moderatorEmail,
                    Email = moderatorEmail,
                    FirstName = "Antonio",
                    LastName = "Raynor",
                    EmailConfirmed = true
                };
                BlogUser? moduser = await userManager.FindByEmailAsync(moderatorUser.Email!);

                if (moduser == null)
                {
                    await userManager.CreateAsync(moderatorUser, moderatorPassword!);
                    await userManager.AddToRoleAsync(moderatorUser, _moderatorRole!); //add mod role to modUser
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Login User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }
    }
}

