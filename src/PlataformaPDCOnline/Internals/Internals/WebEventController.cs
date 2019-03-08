using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Internals.Internals
{
    public class WebEventController
    {
        public string EventName { private set; get; }
        public string TableName { private set; get; }
        public string UidName { private set; get; }
        public string SuscriptioName { private set; get; }

        public WebEventController(Dictionary<string, object> row)
        {
            this.EventName = row.GetValueOrDefault("eventname").ToString();
            this.TableName = row.GetValueOrDefault("tablename").ToString();
            this.UidName = row.GetValueOrDefault("uidname").ToString();
            //this.SuscriptioName = row.GetValueOrDefault("").ToString();
        }

        public override string ToString()
        {
            return "EventName: " + this.EventName + ", TableName: " + this.TableName + ", UidName: " + this.UidName+ ", SuscriptionName: " + this.SuscriptioName;
        }
    }
}
