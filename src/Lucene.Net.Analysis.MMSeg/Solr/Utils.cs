using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using Lucene.Net;

namespace Lucene.Net.Analysis.MMSeg
{
    public class Utils
    {
        public static Dictionary GetDic(string dicPath, ResourceReader loader)
        {
            if (File.Exists(dicPath))
                return Dictionary.getInstance(new FileInfo(dicPath));
            return Dictionary.getInstance();
        }
    }
}
