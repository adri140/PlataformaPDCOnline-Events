using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Internals.Internals
{
    public class WebEventController
    {
        public static List<WebEventController> allSuscriptions;

        public string EventName { private set; get; }
        public string TableName { private set; get; }
        public string UidName { private set; get; }
        public string SuscriptionName { private set; get; }

        public WebEventController(Dictionary<string, object> row)
        {
            this.EventName = row.GetValueOrDefault("eventname").ToString();
            this.TableName = row.GetValueOrDefault("tablename").ToString();
            this.UidName = row.GetValueOrDefault("uidname").ToString();
            //this.SuscriptionName = row.GetValueOrDefault("").ToString();
        }

        public override string ToString()
        {
            return "EventName: " + this.EventName + ", TableName: " + this.TableName + ", UidName: " + this.UidName+ ", SuscriptionName: " + this.SuscriptionName;
        }

        //recoge todas las suscripciones de la base de datos, y las añade a una lista de WebEventController
        public static int GetAllSuscriptions()
        {
            List<Dictionary<string, object>> tableEvents = ConsultasPreparadas.Singelton().GetEvents();

            allSuscriptions = new List<WebEventController>();
            foreach (Dictionary<string, object> row in tableEvents)
            {
                allSuscriptions.Add(new WebEventController(row));
            }

            return allSuscriptions.Count;
        }

        public static WebEventController GetController(Event evento)
        {
            WebEventController selectedController = null;
            foreach(WebEventController controller in allSuscriptions)
            {
                if(evento.GetType().Name.Equals(controller.EventName))
                {
                    selectedController = controller;
                    break;
                }
            }
            return selectedController;
        }
    }
}
