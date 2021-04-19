using EmailSender.API.Consumers;
using EmailSender.API.Helper;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Queue.ConstNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                x.AddConsumer<EmailMessageConsumer>();
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
                        ep.ConfigureConsumer<EmailMessageConsumer>(provider);
                    });
                }));
            });

            services.AddMassTransitHostedService();
        }

        public static void AddCustomSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("smtp"));
        }
    }
}
