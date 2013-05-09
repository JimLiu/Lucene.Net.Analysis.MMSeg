using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis.MMSeg;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// Maximum Matching.
    /// chunk中各个词的长度之和
    /// </summary>
    public class MaxMatchRule : Rule
    {
        int maxLen;

        public override void AddChunk(Chunk chunk)
        {
            if (chunk.Len >= maxLen)
            {
                maxLen = chunk.Len;
                base.AddChunk(chunk);
            }
        }

        protected override bool IsRemove(Chunk chunk)
        {
            return chunk.Len < maxLen;
        }

        public override void Reset()
        {
            maxLen = 0;
            base.Reset();
        }
    }
}
