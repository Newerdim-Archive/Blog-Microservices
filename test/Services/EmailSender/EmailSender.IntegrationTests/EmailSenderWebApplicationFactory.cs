using EmailSender.API.Wrappers;
using EmailSender.IntegrationTests.Mock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;

namespace EmailSender.IntergrationTests
{
    public class EmailSenderWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(ISmtpClientWrapper));

                services.Remove(descriptor);

                services.AddTransient<ISmtpClientWrapper, FakeSmtpClientWrapper>();
            });

            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.Test.json");

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(configPath);
            });
        }
    }
}