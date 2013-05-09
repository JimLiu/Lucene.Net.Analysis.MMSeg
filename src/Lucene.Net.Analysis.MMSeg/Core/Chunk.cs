using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// Chunk
    /// </summary>
    /// <remarks>
    ///     它是MMSeg分词算法中一个关键的概念。Chunk中包含依据上下文分出的一组词和相关的属性，包括长度(Length)、平均长度(Average Length)、标准差的平方(Variance)和自由语素度(Degree Of Morphemic Freedom)。
    /// </remarks>
    public class Chunk
    {
        Word[] words = new Word[3];

        int count = -1;

        int len = -1;

        double avgLen = -1;

        double variance = -1;

        int sumDegree = -1;

        /// <summary>
        /// Word Length
        /// </summary>
        public int Len
        {
            get
            {
                if (len < 0)
                {
                    len = 0;
                    count = 0;
                    foreach(Word word in words)
                    {
                        if (word != null)
                        {
                            len += word.Length;
                            count++;
                        }
                    }
                }
                return len;
            }
        }

        /// <summary>
        /// 有多少个词，最多3个
        /// </summary>
        public int Count
        {
            get
            {
                if (count < 0)
                {
                    count = 0;
                    foreach (Word word in words)
                    {
                        if (word != null)
                            count++;
                    }
                }
                return count;
            }
            set { count = value; }
        }

        /// <summary>
        /// Largest Average Word Length
        /// </summary>
        public double AvgLen
        {
            get
            {
                if (avgLen < 0)
                    avgLen = (double)Len / (double)Count;
                return avgLen;
            }
        }

        /// <summary>
        /// Variance of Word Lengths 标准差的平方
        /// </summary>
        public double Variance
        {
            get
            {
                if (variance < 0)
                {
                    double sum = 0;
                    foreach (Word word in words)
                    {
                        if (word != null)
                            sum += Math.Pow(word.Length - AvgLen, 2);
                    }
                    variance = sum / this.Count;
                }
                return variance;
            }
        }

        /// <summary>
        /// sum of degree of morphemic freedom of one-character
        /// </summary>
        public int SumDegree
        {
            get
            {
                if (sumDegree < 0)
                {
                    int sum = 0;
                    foreach (Word word in words)
                    {
                        if (word != null && word.Degree > -1)
                        {
                            sum += word.Degree;
                        }
                    }
                    sumDegree = sum;
                }
                return sumDegree;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Word word in words)
            {
                if (word != null)
                    builder.Append(word.String).Append("_");
            }
            return builder.ToString();
        }

        public String FactorString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[").Append("len=").Append(Len).Append(", ");
                builder.Append("avgLen=").Append(AvgLen).Append(", ");
                builder.Append("variznce=").Append(Variance).Append(", ");
                builder.Append("sum100log=").Append(SumDegree).Append("]");
                return builder.ToString();
            }
        }

        public Word[] Words
        {
            get { return words; }
            set { words = value; count = words.Length; }
        }
    }
}
