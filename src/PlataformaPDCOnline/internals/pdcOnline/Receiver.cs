﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pdc.Hosting;
using Pdc.Integration.BoundaryContext;
using Pdc.Integration.Denormalization;
using Pdc.Messaging.ServiceBus;
using PlataformaPDCOnline.Editable.EventsHandlers;
using PlataformaPDCOnline.Editable.pdcOnline.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlataformaPDCOnline.internals.pdcOnline
{
    public class Receiver
    {

        private readonly IConfiguration configuration;
        private IHostedService boundedContext;
        private IServiceScope scope;

        public Receiver()
        {
            configuration = GetConfiguration();
            var services = GetBoundedContextServices();

            RunServices(services);
        }

        private async void RunServices(IServiceProvider services)
        {
            Console.WriteLine("Servicio Iniciadondo..");
            using (scope = services.CreateScope())
            {
                boundedContext = services.GetRequiredService<IHostedService>();
                
                await boundedContext.StartAsync(default);
            }
            Console.WriteLine("Iniciado..");
        }

        public async void EndServices()
        {
            using (scope)
            {
                await boundedContext.StopAsync(default);
            }
        }

        private static IConfiguration GetConfiguration()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    /*{ "DistributedRedisCache:InstanceName", "Cache." },
                    { "RedisDistributedLocks:InstanceName", "Locks." },
                    { "DocumentDBPersistence:Database", "Tests" },
                    { "DocumentDBPersistence:Collection", "Events" },
                    { "ProcessManager:Sender:EntityPath", "core-test-commands" },
                    { "BoundedContext:Publisher:EntityPath", "core-test-events" },
                    { "CommandHandler:Receiver:EntityPath", "core-test-commands" },*/
                    { "Denormalization:Subscribers:0:EntityPath", "core-test-events" },
                    { "Denormalization:Subscribers:0:SubscriptionName", "core-test-events-denormalizers" }
                })
                .AddUserSecrets(assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

#if !DEBUG
            return new ConfigurationBuilder()
                .AddConfiguration(c)
                .AddAzureKeyVault(c["AzureKeyVault:Uri"], c["AzureKeyVault:ClientId"], c["AzureKeyVault:ClientSecret"])
                .Build();
#else
            return c;
#endif
        }

        private IServiceProvider GetBoundedContextServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddDebug());

            services.AddAzureServiceBusEventSubscriber(
                builder =>
                {
                    builder.AddEventHandler<CustomerCreated, CustomerDenormalizer>();

                    builder.AddEventHandler<WebUserCreated, WebUserCreatedHandler>();
                    builder.AddEventHandler<WebUserUpdated, WebUserUpdatedHandler>();
                    builder.AddEventHandler<WebUserDeleted, WebUserDeletedHandler>();

                    builder.AddEventHandler<WebAccessGroupCreated, WebAccessGroupCreatedHandler>();
                },
                new Dictionary<string, Action<EventBusOptions>>
                {
                    ["Core"] = options => configuration.GetSection("Denormalization:Subscribers:0").Bind(options),
                });

            services.AddHostedService<HostedService>();

            return services.BuildServiceProvider();
        }

    }
}
