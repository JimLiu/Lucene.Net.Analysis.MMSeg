using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis.MMSeg;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    ///  Smallest Variance of Word Lengths.
    ///  标准差的平方
    /// </summary>
    public class SmallestVarianceRule : Rule
    {
        double smallestVariance = Double.MaxValue;

        public override void AddChunk(Chunk chunk)
        {
            if (chunk.Variance <= smallestVariance)
            {
                smallestVariance = chunk.Variance;
                base.AddChunk(chunk);
            }
        }

        public override void Reset()
        {
            smallestVariance = double.MaxValue;
            base.Reset();
        }

        protected override bool IsRemove(Chunk chunk)
        {
            return chunk.Variance > smallestVariance;
        }
    }
}
