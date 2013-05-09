using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 最多分词方式
    /// </summary>
    public class MaxWordAnalyzer : MMSegAnalyzer
    {
        public MaxWordAnalyzer() : base() { }

        public MaxWordAnalyzer(string path) : base(path) { }

        public MaxWordAnalyzer(FileInfo path) : base(path) { }

        public MaxWordAnalyzer(Dictionary dic) : base(dic) { }

        public new Seg NewSeg { get { return new MaxWordSeg(dic); } }
    }
}
