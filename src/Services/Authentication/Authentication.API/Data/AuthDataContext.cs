using Authentication.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Data
{
    public class AuthDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }


        public AuthDataContext(DbContextOptions<AuthDataContext> options) 
            : base(options) { }
    }
}
