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
            var rabbitMqSettings = new RabbitMqSettings();
            section.Bind(rabbitMqSettings);

            services.AddMassTransit(x =>
            {
                x.AddConsumer<SendEmailConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Uri, x =>
                    {
                        x.Username(rabbitMqSettings.Username);
                        x.Password(rabbitMqSettings.Password);
                    });

                    cfg.ReceiveEndpoint(EmailSenderQueue.SendEmail, e =>
                    {
                        e.ConfigureConsumer<SendEmailConsumer>(context);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddMassTransitHostedService(true);
        }

        public static void AddCustomSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        }

        public static void AddCustomService(this IServiceCollection services)
        {
            services.AddTransient<IEmailSenderService, EmailSenderService>();
        }
    }
}