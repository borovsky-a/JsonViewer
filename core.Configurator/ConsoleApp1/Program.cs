using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var logger = FT.Common.SafeLog.Default;
            var path =logger.Sink.GetDefaultStorePath();
            logger.Write("zdarova");
            Console.ReadLine();
        }
    }
}
