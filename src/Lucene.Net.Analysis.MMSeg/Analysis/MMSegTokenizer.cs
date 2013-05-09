using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Analysis.MMSeg;

namespace Lucene.Net.Analysis.MMSeg
{
    public class MMSegTokenizer : Tokenizer
    {
        MMSeg mmSeg;
        ITermAttribute termAtt;
        IOffsetAttribute offsetAtt;
        ITypeAttribute typeAtt;

        /*
         * 此处忽略调用base(input);因调用后input的position会被移动
         * by zh
         */
        public MMSegTokenizer(Seg seg, TextReader input)
            : base(input)
        {
            mmSeg = new MMSeg(input, seg);
            termAtt = AddAttribute<ITermAttribute>();
            offsetAtt = AddAttribute<IOffsetAttribute>();
            typeAtt = AddAttribute<ITypeAttribute>();
        }

        public override void Reset()
        {
            mmSeg.reset(this.input);
        }

        public override bool IncrementToken()
        {
            ClearAttributes();
            Word word = mmSeg.Next();
            if (word != null)
            {
                termAtt.SetTermBuffer(word.Sen, word.WordOffset, word.Length);
                offsetAtt.SetOffset(word.StartOffset, word.EndOffset);
                typeAtt.Type = word.Type;
                return true;
            }
            else
            {
                End();
                return false;
            }
        }
    }
}
