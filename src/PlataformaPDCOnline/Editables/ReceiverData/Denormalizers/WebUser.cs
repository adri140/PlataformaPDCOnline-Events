using Pdc.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Editables.ReceiverData.Denormalizers
{
    public class WebUser : View
    {
        public string userid { set; get; }
        public string username { set; get; }
        public string usercode { set; get; }
    }
}
