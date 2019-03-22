using PlataformaPDCOnline.internals.pdcOnline;
using System;

namespace PlataformaPDCOnline
{
    class Program
    {
        static void Main(string[] args)
        {
            Receiver rec = new Receiver();

            Console.WriteLine("Press to Stop Services...");
            Console.ReadLine();

            rec.EndServices();
        }
    }
}
