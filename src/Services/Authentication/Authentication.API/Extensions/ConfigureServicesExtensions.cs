using Authentication.API.Data;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Providers;
using Authentication.API.Publishers;
using Authentication.API.Services;
using Authentication.API.Validators;
using EmailSender.API.Helper;
using FluentValidation;
using MassTransit;
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

        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddTransient<IDateProvider, DateProvider>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ITokenService, TokenService>();

            services.AddTransient<IEmailPublisher, EmailPublisher>();
            services.AddTransient<IUserPublisher, UserPublisher>();
        }

        public static void AddCustomMassTransit(this IServiceCollection services, IConfigurationSection section)
        {
            var rabbitMqSettings = new RabbitMqSettings();
            section.Bind(rabbitMqSettings);

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Uri, x =>
                    {
                        x.Username(rabbitMqSettings.Username);
                        x.Password(rabbitMqSettings.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddMassTransitHostedService();
        }

        public static void AddCustomSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
            services.Configure<CompanySettings>(configuration.GetSection("CompanySettings"));
        }
    }
}
