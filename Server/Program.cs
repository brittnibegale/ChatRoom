using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            TextLogger logger = new TextLogger();
            new Server(logger).Run();
            Console.ReadLine();
        }
    }
}
