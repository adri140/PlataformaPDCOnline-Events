using Pdc.Messaging;
using PlataformaPDCOnline.Editable.Events;
using PlataformaPDCOnline.Editable.tableClass;
using PlataformaPDCOnline.Internals.Internals;
using PlataformaPDCOnline.pdcOnline.extended;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.Editable.Denormalizers
{
    public class WebUserDenormalizer : PlataformaDenormalizer<WebUser>, IEventHandler<WebUserCreated>, IEventHandler<WebUserUpdated>, IEventHandler<WebUserDeleted>
    {
        /*public WebUserDenormalizer(PurchaseOrdersDbContext dbContext) : base(dbContext) { }*/

        public async Task HandleAsync(WebUserCreated message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("WebUserCreated recibido");
            WebEventController controller = WebEventController.GetController(message);
            await ConsultasPreparadas.Singelton().RunEventCommitCommit(message, controller);
        }

        public async Task HandleAsync(WebUserUpdated message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("WebUserUpdated recibido");
            WebEventController controller = WebEventController.GetController(message);
            await ConsultasPreparadas.Singelton().RunEventCommitCommit(message, controller);
        }

        public async Task HandleAsync(WebUserDeleted message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("WebUserDeleted recibido");
            WebEventController controller = WebEventController.GetController(message);
            await ConsultasPreparadas.Singelton().RunEventCommitCommit(message, controller);
        }
    }
}
