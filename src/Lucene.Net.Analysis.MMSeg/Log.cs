using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    internal class Log
    {
        public static void Info(string format, params object[] values)
        {
            Console.WriteLine(format, values);
        }
    }
}
