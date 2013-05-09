using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 分词抽像类
    /// </summary>
    public abstract class Seg
    {
        protected Dictionary dic;

        public Seg(Dictionary dic)
        {
            this.dic = dic;
        }

        /// <summary>
        /// 输出chunks,调试用
        /// </summary>
        /// <param name="chunks"></param>
        protected void PrintChunk(List<Chunk> chunks)
        {
            foreach (Chunk ck in chunks)
            {
                Console.WriteLine("{0}->{1}", ck, ck.FactorString);
            }
        }

        public bool IsUnit(int codePoint)
        {
            return dic.isUnit((char)codePoint);
        }

        /// <summary>
        /// 查找chs[offset]后面的tailLen个char是否为词
        /// </summary>
        /// <param name="chs"></param>
        /// <param name="offset"></param>
        /// <param name="tailLen"></param>
        /// <returns>返回chs[offset]字符结点下的词尾索引号,没找到返回-1</returns>
        protected int Search(char[] chs, int offset, int tailLen)
        {
            if (tailLen == 0)
                return -1;
            CharNode cn = dic.head(chs[offset]);
            return Search(cn, chs, offset, tailLen);
        }

        /// <summary>
        /// 没有数组的复制
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="chs"></param>
        /// <param name="offset"></param>
        /// <param name="tailLen"></param>
        /// <returns></returns>
        protected int Search(CharNode cn, char[] chs, int offset, int tailLen)
        {
            if (tailLen == 0 || cn == null) 
                return -1;
            return dic.search(cn, chs, offset, tailLen);
        }

        /// <summary>
        /// 最大匹配,从chs[offset]开始匹配，同时把chs[offset]的字符终点保存在cns[cnIdx]
        /// </summary>
        /// <param name="cns"></param>
        /// <param name="cnIdx"></param>
        /// <param name="chs"></param>
        /// <param name="offset"></param>
        /// <returns>最大匹配到的词尾长,>0 找到</returns>
        protected int MaxMatch(CharNode[] cns, int cnIdx, char[] chs, int offset)
        {
            CharNode cn = null;
            if (offset < chs.Length)
                cn = dic.head(chs[offset]);
            cns[cnIdx] = cn;
            return dic.maxMatch(cn, chs, offset);
        }

        protected void MaxMatch(CharNode[] cns, int cnIdx, char[] chs, int offset, List<int>[] tailLens, int tailLensIdx)
        {
            CharNode cn = null;
            if (offset < chs.Length)
                cn = dic.head(chs[offset]);
            cns[cnIdx] = cn;
            dic.maxMatch(cn, tailLens[tailLensIdx], chs, offset);
        }

        /// <summary>
        /// 对句子sen进行分词
        /// </summary>
        /// <param name="sen"></param>
        /// <returns></returns>
        public abstract Chunk Segment(Sentence sen);
    }
}
