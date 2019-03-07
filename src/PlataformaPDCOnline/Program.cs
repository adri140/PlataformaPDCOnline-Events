using PlataformaPDCOnline.Internals.pdcOnline.Receiver;
using System;
using System.Threading;

namespace PlataformaPDCOnline
{
    class Program
    {
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
    }
}
