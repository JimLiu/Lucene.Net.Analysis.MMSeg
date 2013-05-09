using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 默认使用max-word
    /// <see cref="SimpleAnalyzer"/>, <see cref="ComplexAnalyzer"/>, <see cref="MaxWordAnalyzer"/>
    /// </summary>
    public class MMSegAnalyzer : Analyzer
    {
        protected Dictionary dic;

        public MMSegAnalyzer()
        {
            dic = Dictionary.getInstance();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">词库目录</param>
        public MMSegAnalyzer(string path)
        {
            dic = Dictionary.getInstance(path);
        }

        public MMSegAnalyzer(FileInfo path)
        {
            dic = Dictionary.getInstance(path);
        }

        public MMSegAnalyzer(Dictionary p_dic)
            : base()
        {
            dic = p_dic;
        }


        protected Seg NewSeg 
        { 
            get { 
                return new MaxWordSeg(dic); 
            } 
        }

        public Dictionary Dict 
        { 
            get { 
                return dic; 
            } 
        }

        /*
        public override TokenStream ReusableTokenStream(string fieldName, System.IO.TextReader reader)
        {
            MMSegTokenizer mmsegTokenizer = (MMSegTokenizer)base.PreviousTokenStream;
            if (mmsegTokenizer == null)
            {
                mmsegTokenizer = new MMSegTokenizer(NewSeg, reader);
                base.PreviousTokenStream = mmsegTokenizer;
            }
            else
            {
                mmsegTokenizer.Reset(reader);
            }
            return mmsegTokenizer;
        }
        */

        public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
        {
            Lucene.Net.Analysis.TokenStream ts = new MMSegTokenizer(NewSeg, reader);
            return ts;
        }
        
    }
}
