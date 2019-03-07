using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pdc.Hosting;
using Pdc.Messaging;
using Pdc.Messaging.ServiceBus;
using PlataformaPDCOnline.Editable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.Internals.pdcOnline.Receiver
{

    public class SuscriptionsJobs
    {
        private static List<EventSubscriberOptions> SuscripcionesEnBruto = new List<EventSubscriberOptions>();

        public static void AddSuscription(string suscriptionName)
        {
            SuscripcionesEnBruto.Add(new EventSubscriberOptions() { SubscriptionName = suscriptionName });
            ProcessManagerOptions suscripciones = new ProcessManagerOptions() { Subscribers = SuscripcionesEnBruto.ToArray()}; //ojo aqui porque te estas cargando todo lo que haiga
        }

        public static void DeleteSuscriptionIfExist(string suscriptionName)
        {
            EventSubscriberOptions tmp = null;

            foreach (EventSubscriberOptions suscription in SuscripcionesEnBruto)
            {
                if (suscription.SubscriptionName.Equals(suscriptionName)) tmp = suscription;
            }

            if (tmp != null)
            {
                SuscripcionesEnBruto.Remove(tmp);
                ProcessManagerOptions suscripciones = new ProcessManagerOptions() { Subscribers = SuscripcionesEnBruto.ToArray() }; //ojo aqui porque te estas cargando todo lo que haiga
            }
        }
    }

    class Receiver
    {
        private static Receiver receiver;

        public static Receiver Singelton()
        {
            if(receiver == null)
            {
                receiver = new Receiver();
            }

            return receiver;
        }

        private CancellationTokenSource cancellationToken;
        private readonly IConfiguration configuration;

        private Receiver()
        {
            this.configuration = GetConfiguration();
            this.InicializeServices();
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

        private void InicializeServices()
        {
            this.cancellationToken = new CancellationTokenSource();

            // Start in their own thread a Denormalization context that will receive the BoundedContext's events
            var denormalizationServices = GetDenormalizationServices();
            var denormalizationWorker = ExecuteDenormalizationAsync(denormalizationServices, this.cancellationToken.Token);
        }

        private IServiceProvider GetDenormalizationServices()
        {
            var services = new ServiceCollection();

            services.AddDenormalization(options => options.Bind(configuration.GetSection("Denormalization")));
            
            services.AddLogging(builder => builder.AddDebug());
            /*services.AddDbContext<PurchaseOrdersDbContext>(options => options.UseSqlite(connection));*/
            

            //services.AddDenormalizer<Pdc.Integration.Denormalization.Customer, CustomerDenormalizer>();
            //services.AddDenormalizer<Pdc.Integration.Denormalization.CustomerDetail, CustomerDetailDenormalizer>();
            services.AddDenormalizer<WebUser, WebUserDenormalizer>();

            return services.BuildServiceProvider();
        }

        private static async Task ExecuteDenormalizationAsync(IServiceProvider services, CancellationToken cancellationToken)
        {

            /*using (var scope = services.CreateScope())
            {
                var dbContext = services.GetRequiredService<PurchaseOrdersDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }*/

            var denormalization = services.GetRequiredService<IHostedService>();

            try
            {
                await denormalization.StartAsync(default);

                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    cancellationToken);

            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                await denormalization.StopAsync(default);
            }
        }

        public void Stop()
        {
            this.cancellationToken.Cancel();
        }
    }
}

