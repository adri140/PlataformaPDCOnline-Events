using PlataformaPDCOnline.Internals.Internals;
using PlataformaPDCOnline.Internals.pdcOnline.Receiver;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PlataformaPDCOnline
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcom To Job And Events");

            StartFunction();

            Boolean run = true;

            do
            {
                string commandLine = Console.ReadLine();
                switch (commandLine)
                {
                    case "stop":
                        run = false;
                        Receiver.Singelton().Stop();
                        break;
                    default:
                        Console.WriteLine("Comando desconocido.");
                        break;
                }
            } while (run);
        }

        private static void StartFunction()
        {
            if(WebEventController.GetAllSuscriptions() >  0)
            {
                StartReceiver();
            }
        }

        private static void StartReceiver()
        {
            Receiver.Singelton();
        }
    }
}
