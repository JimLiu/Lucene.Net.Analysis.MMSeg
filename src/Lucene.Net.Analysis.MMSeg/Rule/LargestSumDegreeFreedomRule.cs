using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis.MMSeg;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// Largest Sum of Degree of Morphemic Freedom of One-Character.
    /// 各单勃词词频的对数之和*100
    /// </summary>
    public class LargestSumDegreeFreedomRule : Rule
    {
        int largesetSumDegree = Int32.MinValue;

        public override void AddChunk(Chunk chunk)
        {
            if (chunk.SumDegree >= largesetSumDegree)
            {
                largesetSumDegree = chunk.SumDegree;
                base.AddChunk(chunk);
            }
        }

        public override void Reset()
        {
            largesetSumDegree = Int32.MinValue;
            base.Reset();
        }

        protected override bool IsRemove(Chunk chunk)
        {
            return chunk.SumDegree < largesetSumDegree;
        }
    }
}
