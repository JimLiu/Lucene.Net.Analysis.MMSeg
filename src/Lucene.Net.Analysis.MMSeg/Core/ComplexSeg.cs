using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis.MMSeg;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 正向最大匹配,加四个过滤规则的分词方式
    /// </summary>
    public class ComplexSeg : Seg
    {
        MaxMatchRule mmr = new MaxMatchRule();
        List<Rule> otherRules = new List<Rule>();
        static bool showChunk = false;

        public ComplexSeg(Dictionary dic)
            : base(dic)
        {
            otherRules.Add(new LargestAvgLenRule());
            otherRules.Add(new SmallestVarianceRule());
            otherRules.Add(new LargestSumDegreeFreedomRule());
        }

        public override Chunk Segment(Sentence sen)
        {
            char[] chs = sen.Text;
            int[] tailLen = new int[3];//记录词的尾长
            List<int>[] tailLens = new List<int>[2];//记录词尾部允许的长度
            for (int i = 0; i < 2; i++)
            {
                tailLens[i] = new List<int>();
            }
            CharNode[] cns = new CharNode[3];

            //每个词在SEN的开始位置
            int[] offsets = new int[3];
            mmr.Reset();
            if (!sen.IsFinish)
            {
                if (showChunk)
                {
                    Console.WriteLine();
                }
                int maxLen = 0;
                offsets[0] = sen.Offset;
                //Console.WriteLine("{0}:{1}", sen.Offset, new String(sen.Text));
                /*
                 * 遍历所有不同词长,还不是从最大到0(w[0]=maxLen(chs,offsets[0]);w[0]>=0;w[0]--)
                 * 可以减少一部分多余的查找
                 */
                MaxMatch(cns, 0, chs, offsets[0], tailLens, 0);
                for (int aIdx = tailLens[0].Count - 1; aIdx >= 0; aIdx--)
                {
                    tailLen[0] = tailLens[0][aIdx];
                    //第二个词的开始位置
                    offsets[1] = offsets[0] + 1 + tailLen[0];
                    MaxMatch(cns, 1, chs, offsets[1], tailLens, 1);
                    for (int bIdx = tailLens[1].Count - 1; bIdx >= 0; bIdx--)
                    {
                        tailLen[1] = tailLens[1][bIdx];
                        offsets[2] = offsets[1] + 1 + tailLen[1];

                        //第三个词只需要最长的
                        tailLen[2] = MaxMatch(cns, 2, chs, offsets[2]);
                        int sumChunkLen = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            sumChunkLen += tailLen[i] + 1;
                        }
                        Chunk ck = null;
                        if (sumChunkLen >= maxLen)
                        {
                            maxLen = sumChunkLen;
                            ck = CreateChunk(sen, chs, tailLen, offsets, cns);
                            mmr.AddChunk(ck);
                        }
                        if (showChunk)
                        {
                            if (ck == null)
                            {
                                ck = CreateChunk(sen, chs, tailLen, offsets, cns);
                                mmr.AddChunk(ck);
                            }
                            Console.WriteLine(ck);
                        }
                    }
                }
                //maxLen个字符已经处理完
                sen.AddOffset(maxLen);
                //Console.WriteLine("max:{0}", maxLen);
                List<Chunk> chunks = mmr.RemainChunks();
                foreach (Rule rule in otherRules)
                {
                    if (showChunk)
                    {
                        Console.WriteLine("---------filter before {0} -----------", rule);
                        PrintChunk(chunks);
                    }
                    if (chunks.Count <= 1)
                        break;

                    rule.Reset();
                    rule.AddChunks(chunks);
                    chunks = rule.RemainChunks();
                }
                if (showChunk)
                {
                    Console.WriteLine("------------remainChunks--------");
                    PrintChunk(chunks);
                }
                if (chunks.Count > 0)
                    return chunks[0];
            }

            return null;
        }

        Chunk CreateChunk(Sentence sen, char[] chs, int[] tailLen, int[] offsets, CharNode[] cns)
        {
            Chunk ck = new Chunk();
            for (int i = 0; i < 3; i++)
            {
                if (offsets[i] < chs.Length)
                {
                    ck.Words[i] = new Word(chs, sen.StartOffset, offsets[i], tailLen[i] + 1);
                    if (tailLen[i] == 0) //单字的要取得"字频计算出自由度"
                    {
                        CharNode cn = cns[i];
                        if (cn != null)
                        {
                            ck.Words[i].Degree = cn.Freq;
                        }
                    }
                }
            }
            return ck;
        }

        public static bool ShowChunk
        {
            get { return showChunk; }
            set { showChunk = value; }
        }
    }
}
