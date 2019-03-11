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

            Boolean status = StartFunction();

            Boolean run = true;

            do
            {
                string commandLine = Console.ReadLine();
                switch (commandLine)
                {
                    case "stop":
                        run = false;
                        if(status) Receiver.Singelton().Stop();
                        break;
                    default:
                        Console.WriteLine("Comando desconocido.");
                        break;
                }
            } while (run);
        }

        private static Boolean StartFunction()
        {
            if(WebEventController.GetAllSuscriptions() >  0)
            {
                StartReceiver();
                return false;
            }
            return true;
        }

        private static void StartReceiver()
        {
            Receiver.Singelton();
        }
    }
}
