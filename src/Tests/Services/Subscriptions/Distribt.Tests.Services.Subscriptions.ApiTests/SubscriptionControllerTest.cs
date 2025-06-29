using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Distribt.Services.Subscriptions.Dtos;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.Publisher.Integration;
using Distribt.Shared.Discovery;
using Distribt.Shared.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;


namespace Distribt.Tests.Services.Subscriptions.ApiTests;

public class SubscriptionControllerTest
{

    [Fact]
    public async Task WhenSubscriptionApi_Then_EventPublished()
    {
        SubscriptionApi subscriptionApi = new SubscriptionApi();
        HttpClient client = subscriptionApi.CreateClient();

        SubscriptionDto subscriptionDto = new("Email");
        JsonContent dtoAsJson = JsonContent.Create(subscriptionDto);
        var response = await client.PostAsync("/subscription", dtoAsJson);
        response.EnsureSuccessStatusCode();

        Assert.Single(subscriptionApi.FakeIntegrationPublisher.Objects);
        SubscriptionDto dtoSent = subscriptionApi.FakeIntegrationPublisher.Objects.First() as SubscriptionDto ?? throw new InvalidOperationException();

        Assert.Equal(subscriptionDto.Email, dtoSent.Email);
    }

    class SubscriptionApi : WebApplicationFactory<Program>
    {
        public FakeIntegrationPublisher FakeIntegrationPublisher;

        public SubscriptionApi()
        {
            FakeIntegrationPublisher = new FakeIntegrationPublisher();
        }
        
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPostConfigureOptions<VaultSettings>>();
                services.PostConfigure<VaultSettings>(options =>
                {
                    options.UpdateUrl("new-value");
                });

                services.RemoveAll<IServiceDiscovery>();
                var fakeDiscovery = new FakeDiscovery();
                services.AddSingleton<IServiceDiscovery>(fakeDiscovery);
                
                services.AddSingleton<IIntegrationMessagePublisher>(FakeIntegrationPublisher);
            });
            return base.CreateHost(builder);
        }
    }

    public class FakeVaultSettings : IPostConfigureOptions<VaultSettings>
    {
        public void PostConfigure(string? name, VaultSettings options)
        {
            options.UpdateUrl("http://url.com");
        }
    }
    
    public class FakeDiscovery : IServiceDiscovery
    {
        public Task<string> GetFullAddress(string serviceKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"{serviceKey}-address");
        }

        public Task<DiscoveryData> GetDiscoveryData(string serviceKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DiscoveryData("serviceKey", 1));
        }
    }
    
    
    public class FakeIntegrationPublisher :IIntegrationMessagePublisher
    {
        public List<object> Objects = new List<object>();
        public Task Publish(object message, Metadata? metadata = null, string? routingKey = null,
            CancellationToken cancellationToken = default)
        {
            Objects.Add(message);
            return Task.CompletedTask;
        }

        public Task PublishMany(IEnumerable<object> messages, Metadata? metadata = null, string? routingKey = null,
            CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}