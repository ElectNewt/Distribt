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
using Microsoft.Extensions.DependencyInjection;
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
                services.AddSingleton<IIntegrationMessagePublisher>(FakeIntegrationPublisher);
            });
            return base.CreateHost(builder);
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