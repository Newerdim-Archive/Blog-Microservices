using Authentication.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;

namespace Authentication.API.Extensions
{
    public static class ConfigureServicesExtensions
    {
        public static void AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            services.AddDbContextPool<AuthDataContext>(
                opt => opt
                    .UseMySql(
                        connectionString,
                        ServerVersion.AutoDetect(connectionString),
                        mySqlOptions => mySqlOptions
                            .CharSetBehavior(CharSetBehavior.NeverAppend))
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );
        }
    }
}
