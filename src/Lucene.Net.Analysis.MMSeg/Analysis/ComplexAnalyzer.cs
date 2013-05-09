using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.MMSeg;

namespace Lucene.Net.Analysis.MMSeg
{
    public class ComplexAnalyzer : MMSegAnalyzer
    {
        public ComplexAnalyzer() : base() { }

        public ComplexAnalyzer(string path) : base(path) { }

        public ComplexAnalyzer(FileInfo path) : base(path) { }

        public ComplexAnalyzer(Dictionary dic) : base(dic) { }

        public new Seg NewSeg { get { return new ComplexSeg(dic); } }
    }
}
