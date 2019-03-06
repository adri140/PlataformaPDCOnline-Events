using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.Internals.pdcOnline.Receiver
{
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
            services.AddDbContext<PurchaseOrdersDbContext>(options => options.UseSqlite(connection));

            services.AddDenormalizer<Pdc.Integration.Denormalization.Customer, CustomerDenormalizer>();
            services.AddDenormalizer<Pdc.Integration.Denormalization.CustomerDetail, CustomerDetailDenormalizer>();

            return services.BuildServiceProvider();
        }

        private static async Task ExecuteDenormalizationAsync(IServiceProvider services, CancellationToken cancellationToken)
        {

            using (var scope = services.CreateScope())
            {
                var dbContext = services.GetRequiredService<PurchaseOrdersDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

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
    }
}

