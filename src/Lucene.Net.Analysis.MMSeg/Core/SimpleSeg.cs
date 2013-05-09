using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 正向最大匹配的分词方式
    /// </summary>
    public class SimpleSeg : Seg
    {
        public SimpleSeg(Dictionary dic) : base(dic) { }

        public override Chunk Segment(Sentence sen)
        {
            Chunk chunk = new Chunk();
            char[] chs = sen.Text;
            for (int k = 0; k < 3 && !sen.IsFinish; k++)
            {
                int offset = sen.Offset;
                int maxLen = 0;

                //有了 key tree 的支持可以从头开始 max match
                maxLen = dic.maxMatch(chs, offset);
                chunk.Words[k] = new Word(chs, sen.StartOffset, offset, maxLen + 1);
                offset += maxLen + 1;
                sen.Offset = offset;
            }
            return chunk;
        }
    }
}
