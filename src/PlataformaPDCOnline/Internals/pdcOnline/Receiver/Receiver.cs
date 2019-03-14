
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdc.Hosting;
using Pdc.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlataformaPDCOnline.Internals.pdcOnline.Receiver
{
    public class Receiver
    {
        private readonly IConfiguration configuration;

        public Receiver()
        {
            configuration = GetConfiguration();
        }

        private static IConfiguration GetConfiguration()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DistributedRedisCache:InstanceName", "Cache." },
                    { "RedisDistributedLocks:InstanceName", "Locks." },
                    { "DocumentDBPersistence:Database", "Tests" },
                    { "DocumentDBPersistence:Collection", "Events" },
                    { "ProcessManager:Sender:EntityPath", "core-test-commands" },
                    { "BoundedContext:Publisher:EntityPath", "core-test-events" },
                    { "CommandHandler:Receiver:EntityPath", "core-test-commands" },
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

            services.AddAzureServiceBusCommandReceiver(
                builder =>
                {
                    builder.AddCommandHandler<CreateCustomer, CreateCustomerHandler>();
                    builder.AddCommandHandler<RenameCustomer, RenameCustomerHandler>();
                    builder.AddCommandHandler<MoveCustomer, MoveCustomerHandler>();
                },
                new Dictionary<string, Action<CommandBusOptions>>
                {
                    ["Core"] = options => configuration.GetSection("CommandHandler:Receiver").Bind(options),
                });

            services.AddAzureServiceBusEventSubscriber(
                builder =>
                {
                    builder.AddDenormalizer<Pdc.Integration.Denormalization.Customer, CustomerDenormalizer>();
                    builder.AddDenormalizer<Pdc.Integration.Denormalization.CustomerDetail, CustomerDetailDenormalizer>();
                },
                new Dictionary<string, Action<EventBusOptions>>
                {
                    ["Core"] = options => configuration.GetSection("Denormalization:Subscribers:0").Bind(options),
                });

            services.AddAggregateRootFactory();
            services.AddUnitOfWork();
            services.AddDocumentDBPersistence(options => configuration.GetSection("DocumentDBPersistence").Bind(options));
            services.AddRedisDistributedLocks(options => configuration.GetSection("RedisDistributedLocks").Bind(options));
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = configuration["DistributedRedisCache:Configuration"];
                options.InstanceName = configuration["DistributedRedisCache:InstanceName"];
            });

            //services.AddDbContext<PurchaseOrdersDbContext>(options => options.UseSqlite(connection));

            services.AddAzureServiceBusCommandSender(options => configuration.GetSection("ProcessManager:Sender").Bind(options));
            services.AddAzureServiceBusEventPublisher(options => configuration.GetSection("BoundedContext:Publisher").Bind(options));

            services.AddHostedService<HostedService>();

            return services.BuildServiceProvider();
        }
    }
}

