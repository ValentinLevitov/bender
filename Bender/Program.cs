using System;

namespace Bender
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(
@"Bender must always be run with a single parameter 'schedule name', for example: 
----
# bender daily
----");
                return;
            }
            Application.Run(args);
        }
    }
}
