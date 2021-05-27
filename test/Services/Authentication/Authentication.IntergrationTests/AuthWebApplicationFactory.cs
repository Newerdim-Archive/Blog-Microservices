using Authentication.API.Data;
using Authentication.API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace Authentication.IntergrationTests
{
    public class AuthWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<AuthDataContext>));

                services.Remove(descriptor);

                services.AddDbContextPool<AuthDataContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;

                var context = scopedServices.GetRequiredService<AuthDataContext>();

                var logger = scopedServices
                    .GetRequiredService<ILogger<AuthWebApplicationFactory<TStartup>>>();

                context.Database.EnsureCreated();

                try
                {
                    SeedUsers(context);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the " +
                        "database. Error: {Message}", ex.Message);
                }
            });

            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.Test.json");

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(configPath);
            });
        }

        private void SeedUsers(AuthDataContext context)
        {
            var users = new User[]
            {
                new User
                {
                    Id = 1,
                    Username = "User1",
                    Email = "User1@mail.com",
                    // Password - User1!@#
                    PasswordHash = new byte[] { 57, 176, 251, 213, 29, 109, 174, 169, 209, 95, 150, 20, 56, 163, 168, 174, 105, 43, 215, 190, 159, 74, 23, 111, 204, 154, 45, 222, 178, 192, 43, 142, 181, 88, 161, 114, 143, 100, 154, 188, 168, 61, 164, 154, 116, 217, 87, 169, 76, 249, 38, 151, 51, 46, 140, 95, 5, 72, 176, 6, 120, 88, 37, 187 },
                    PasswordSalt = new byte[] { 70, 110, 56, 86, 126, 115, 160, 127, 79, 152, 74, 65, 107, 61, 175, 208, 168, 225, 200, 85, 195, 82, 80, 187, 49, 146, 135, 218, 186, 69, 200, 143, 57, 139, 194, 85, 231, 30, 239, 9, 103, 239, 111, 254, 148, 254, 19, 116, 43, 206, 59, 37, 226, 33, 202, 16, 105, 207, 172, 70, 108, 93, 106, 195, 33, 170, 216, 13, 35, 139, 18, 50, 146, 113, 180, 141, 171, 170, 228, 52, 11, 62, 80, 203, 195, 125, 249, 148, 36, 197, 179, 74, 222, 251, 147, 70, 55, 211, 70, 48, 33, 78, 53, 90, 174, 107, 213, 203, 244, 244, 231, 241, 34, 231, 106, 204, 102, 178, 134, 158, 41, 207, 189, 57, 57, 45, 94, 5 }
                },

                new User
                {
                    Id = 2,
                    Username = "User2",
                    Email = "User2@mail.com",
                    // Password - User2!@#
                    PasswordHash = new byte[] { 47, 26, 11, 159, 80, 164, 117, 74, 231, 8, 19, 54, 61, 237, 51, 246, 199, 0, 41, 159, 42, 37, 123, 255, 20, 75, 0, 122, 222, 87, 54, 15, 138, 97, 219, 130, 149, 127, 3, 236, 90, 214, 251, 106, 216, 20, 71, 250, 211, 42, 97, 113, 64, 15, 240, 16, 18, 59, 242, 132, 106, 91, 44, 122 },
                    PasswordSalt = new byte[] { 193, 196, 7, 205, 10, 113, 189, 210, 58, 220, 122, 34, 120, 138, 66, 248, 232, 127, 51, 21, 204, 120, 167, 78, 78, 232, 9, 193, 77, 80, 239, 187, 3, 195, 204, 237, 254, 188, 221, 146, 181, 135, 81, 89, 146, 53, 181, 127, 16, 35, 46, 61, 208, 162, 204, 206, 185, 205, 221, 250, 150, 174, 179, 186, 34, 68, 253, 58, 123, 77, 201, 65, 72, 184, 184, 103, 83, 225, 67, 33, 122, 252, 3, 76, 11, 228, 142, 96, 209, 128, 241, 249, 170, 64, 33, 183, 212, 32, 38, 185, 48, 16, 204, 236, 37, 28, 87, 73, 206, 14, 221, 48, 193, 216, 17, 250, 216, 174, 70, 100, 13, 179, 144, 70, 22, 189, 240, 31 }
                },
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}