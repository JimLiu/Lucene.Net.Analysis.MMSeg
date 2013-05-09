using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// Largest Average Word Length.
    /// 长度(Length)/词数
    /// </summary>
    public class LargestAvgLenRule : Rule
    {
        double largestAvgLen;

        public override void AddChunk(Chunk chunk)
        {
            if (chunk.AvgLen >= largestAvgLen)
            {
                largestAvgLen = chunk.AvgLen;
                base.AddChunk(chunk);
            }
        }

        protected override bool IsRemove(Chunk chunk)
        {
            return chunk.AvgLen < largestAvgLen;
        }

        public override void Reset()
        {
            largestAvgLen = 0;
            base.Reset();
        }
    }
}
