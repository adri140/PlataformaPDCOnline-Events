using PlataformaPDCOnline.Internals.Internals;
using PlataformaPDCOnline.Internals.pdcOnline.Receiver;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PlataformaPDCOnline
{
    class Program
    {
        public static List<WebEventController> controllers;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcom To Job And Events");
            
            Boolean run = true;

            do
            {
                string commandLine = Console.ReadLine();
                switch (commandLine)
                {
                    case "stop":
                        run = false;
                        break;
                    default:
                        Console.WriteLine("Comando desconocido.");
                        break;
                }
            } while (run);
        }

        private static void StartFunction()
        {
            List<Dictionary<string, object>> tableEvents = ConsultasPreparadas.Singelton().GetEvents();

            controllers = new List<WebEventController>();
            foreach(Dictionary<string, object> row in tableEvents)
            {
                controllers.Add(new WebEventController(row));
            }

            if(controllers.Count > 0)
            {
                StartReceiver();
            }
        }

        private static void StartReceiver()
        {

        }
    }
}
