using EmailSender.API.Helper;
using EmailSender.API.Services;
using EventBus.Messages;
using EventBus.QueuesName;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailSender.API.Extensions
{
    public static class ConfigureServicesExtensions
    {
        public static void AddCustomMassTransit(this IServiceCollection services, IConfigurationSection section)
        {
            var rabbitMqConfiguration = new RabbitMqConfiguration();
            section.Bind(rabbitMqConfiguration);

            services.AddMassTransit(x =>
            {
                x.AddConsumer<SendEmailConsumer>();
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(config =>
                {
                    config.UseHealthCheck(provider);
                    config.Host(rabbitMqConfiguration.Uri, h =>
                    {
                        h.Username(rabbitMqConfiguration.Username);
                        h.Password(rabbitMqConfiguration.Password);
                    });
                    config.ReceiveEndpoint(EmailSenderQueue.SendEmail, ep =>
                    {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<SendEmailConsumer>(provider);
                    });
                }));
            });

            services.AddScoped<SendEmailConsumer>();

            services.AddMassTransitHostedService();
        }

        public static void AddCustomSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("smtp"));
        }

        public static void AddCustomService(this IServiceCollection services)
        {
            services.AddTransient<IEmailSenderService, EmailSenderService>();
        }
    }
}