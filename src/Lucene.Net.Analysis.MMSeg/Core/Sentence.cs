using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 句子,在一大串字符中断出连续中文的文本
    /// </summary>
    public class Sentence
    {
        char[] text;
        int startOffset;
        int offset;

        public Sentence()
        {
            text = new char[0];
        }

        public Sentence(char[] text, int startOffset)
        {
            reinit(text, startOffset);
        }

        public void reinit(char[] text, int startOffset)
        {
            this.text = text;
            this.startOffset = startOffset;
            offset = 0;
        }

        public char[] Text
        {
            get { return text; }
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public void AddOffset(int inc)
        {
            offset += inc;
        }

        public bool IsFinish
        {
            get { return offset >= text.Length; }
        }

        public int StartOffset
        {
            get { return startOffset; }
            set { startOffset = value; }
        }
    }
}
