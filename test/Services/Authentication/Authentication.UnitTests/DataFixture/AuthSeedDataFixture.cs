using Authentication.API.Data;
using Authentication.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Authentication.UnitTests.DataFixture
{
    public class AuthSeedDataFixture : IDisposable
    {
        public readonly AuthDataContext Context;

        public AuthSeedDataFixture()
        {
            Context = NewEmptyDb();
            SeedUsers(Context);
        }

        public static AuthDataContext NewEmptyDb()
        {
            var options = new DbContextOptionsBuilder<AuthDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AuthDataContext(options);
        }

        private static void SeedUsers(AuthDataContext context)
        {
            var users = new User[]
            {
                new User
                {
                    Id = 1,
                    Email = "admin@admin.com",
                    Username = "admin",
                    PasswordHash = new byte[] { 0, 1, 2 },
                    PasswordSalt = new byte[] { 3, 4, 5 },
                    ConfirmedEmail = true,
                    Created = DateTimeOffset.UtcNow,
                    LastChange = DateTimeOffset.UtcNow
                },
                new User
                {
                    Id = 2,
                    Email = "user1@user.com",
                    Username = "user1",
                    PasswordHash = new byte[] { 4, 5, 1 },
                    PasswordSalt = new byte[] { 3, 4, 3 },
                    ConfirmedEmail = false,
                    Created = DateTimeOffset.UtcNow,
                    LastChange = DateTimeOffset.UtcNow
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}