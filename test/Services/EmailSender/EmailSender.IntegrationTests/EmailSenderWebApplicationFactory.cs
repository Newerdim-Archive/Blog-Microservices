using EmailSender.API.Wrappers;
using EmailSender.IntegrationTests.Mock;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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

                descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(IBus));

                // Remove IBus because test uses in memory harness
                services.Remove(descriptor);
            });
        }
    }
}