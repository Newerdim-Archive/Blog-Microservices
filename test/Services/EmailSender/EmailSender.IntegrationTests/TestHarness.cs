using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EmailSender.IntegrationTests
{
    public class TestHarness<TConsumer, TRequest> : IDisposable
        where TConsumer : class, IConsumer
        where TRequest : class
    {
        private readonly IConsumerTestHarness<TConsumer> _consumerHarness;
        private readonly IRequestClient<TRequest> _client;
        private readonly InMemoryTestHarness _harness;

        public TestHarness(IServiceCollection requiredServices)
        {
            var provider = requiredServices
               .AddMassTransitInMemoryTestHarness(cfg =>
               {
                   cfg.AddConsumer<TConsumer>();
                   cfg.AddConsumerTestHarness<TConsumer>();
               })
               .BuildServiceProvider(true);

            _harness = provider.GetRequiredService<InMemoryTestHarness>();

            _harness.Start().Wait();

            var bus = provider.GetRequiredService<IBus>();
            _consumerHarness = provider.GetRequiredService<IConsumerTestHarness<TConsumer>>();
            _client = bus.CreateRequestClient<TRequest>();
        }

        public async Task<Response<TResponse>> GetResponse<TResponse>(TRequest message) where TResponse : class
        {
            return await _client.GetResponse<TResponse>(message);
        }

        public async Task<bool> IsHarnessConsumed()
        {
            return await _harness.Consumed.Any<TRequest>();
        }

        public async Task<bool> IsConsumerHarnessConsumed()
        {
            return await _consumerHarness.Consumed.Any<TRequest>();
        }

        public void Dispose()
        {
            _harness.Stop().Wait();

            GC.SuppressFinalize(this);
        }
    }
}
