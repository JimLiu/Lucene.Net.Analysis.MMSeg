using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lucene.Net.Analysis.MMSeg;
using Lucene.Net.Analysis;

namespace Lucene.Net.Analysis.MMSeg
{
    public class SimpleAnalyzer : MMSegAnalyzer
    {
        public SimpleAnalyzer() : base() { }

        public SimpleAnalyzer(string path) : base(path) { }

        public SimpleAnalyzer(FileInfo path) : base(path) { }

        public SimpleAnalyzer(Dictionary dic) : base(dic) { }

        public new Seg NewSeg { get { return new SimpleSeg(dic); } }
    }
}
