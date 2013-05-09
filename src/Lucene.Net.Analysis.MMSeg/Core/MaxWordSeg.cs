using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 最多粉刺,在ComplexSeg基础上把长的词拆
    /// </summary>
    public class MaxWordSeg : ComplexSeg
    {
        public MaxWordSeg(Dictionary dic)
            : base(dic)
        { }

        public override Chunk Segment(Sentence sen)
        {
            Chunk chunk = base.Segment(sen);
            if (chunk != null)
            {
                List<Word> cks = new List<Word>();
                for (int i = 0; i < chunk.Count; i++)
                {
                    Word word = chunk.Words[i];
                    if (word.Length < 3)
                    {
                        cks.Add(word);
                    }
                    else
                    {
                        char[] chs = word.Sen;
                        int offset = word.WordOffset;
                        int n = 0;
                        int wordEnd = word.WordOffset + word.Length;
                        int senStartOffset = word.StartOffset - offset; //sen 在文件中的位置
                        int end = -1;//上一次找到的位置
                        for (; offset < wordEnd - 1; offset++)
                        {
                            int idx = Search(chs, offset, 1);
                            if (idx > -1)
                            {
                                cks.Add(new Word(chs, senStartOffset, offset, 2));
                                end = offset + 2;
                                n++;
                            }
                            else if (offset >= end)
                            {
                                //有单字
                                cks.Add(new Word(chs, senStartOffset, offset, 1));
                                end = offset + 1;
                            }
                        }
                        if (end > -1 && end < wordEnd)
                        {
                            cks.Add(new Word(chs, senStartOffset, offset, 1));
                        }
                    }
                }
                chunk.Words = cks.ToArray();
                chunk.Count = cks.Count;
            }
            return chunk;
        }

    }
}
