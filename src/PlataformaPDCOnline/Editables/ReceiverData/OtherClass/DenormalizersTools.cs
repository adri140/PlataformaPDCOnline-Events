using PlataformaPDCOnline.Internals.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Editables.ReceiverData.OtherClass
{
    public class DenormalizersTools
    {
        //busca el WebEventController en la lista de controladores obtenidos de la base de datos, si no existe devuelve null, ojo!!!!!!
        public static WebEventController GetWebEventController(string eventName)
        {
            WebEventController controller = null;

            foreach(WebEventController internalController in WebEventController.allSuscriptions)
            {
                if (internalController.EventName.Equals(eventName))
                {
                    controller = internalController;
                    break;
                }
            }

            return controller;
        }
    }
}
