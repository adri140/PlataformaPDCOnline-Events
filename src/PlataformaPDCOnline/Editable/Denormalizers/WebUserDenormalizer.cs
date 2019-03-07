using Pdc.Messaging;
using PlataformaPDCOnline.Editable.Events;
using PlataformaPDCOnline.Editable.tableClass;
using PlataformaPDCOnline.pdcOnline.extended;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.Editable.Denormalizers
{
    public class WebUserDenormalizer : MyDenormalizer<WebUser>, IEventHandler<WebUserCreated>
    {
        /*public WebUserDenormalizer(PurchaseOrdersDbContext dbContext) : base(dbContext) { }*/

        public async Task HandleAsync(WebUserCreated message, CancellationToken cancellationToken = default)
        {
            
        }

        public async Task HandleAsync(WebUserUpdated message, CancellationToken cancellationToken = default)
        {

        }

        public async Task HandleAsync(WebUserDeleted message, CancellationToken cancellationToken = default)
        {

        }


    }
}
