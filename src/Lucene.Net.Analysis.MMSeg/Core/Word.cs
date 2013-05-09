using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 类似lucene的token
    /// </summary>
    public class Word
    {
        public static string TYPE_WORD = "word";
        public static string TYPE_LETTER = "letter";
        /*字母开头的"字母或数字"*/
        public static string TYPE_LETTER_OR_DIGIT = "letter_or_digit";
        public static string TYPE_DIGIT = "digit";
        /*数字开头的"字母或数字"*/
        public static string TYPE_DIGIT_OR_LETTER = "digit_or_letter";
        public static string TYPE_LETTER_NUMBER = "letter_number";
        public static string TYPE_OTHER_NUMBER = "other_number";

        int degree = -1;
        int startOffset;

        char[] sen;
        int offset;
        int len;

        string type = TYPE_WORD; //类似 lucene token  的 type

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="startOffset">startOffset word 在整个文本中的偏移位置</param>
        public Word(char[] word, int startOffset)
        {
            this.sen = word;
            this.startOffset = startOffset;
            offset = 0;
            len = word.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="startOffset">startOffset word 在整个文本中的偏移位置</param>
        /// <param name="wordType"></param>
        public Word(char[] word, int startOffset, string wordType)
            : this(word, startOffset)
        {
            this.type = wordType;
        }

        public Word(char[] word, int startOffset, int offset, int len)
            : this(word, startOffset)
        {
            this.offset = offset;
            this.len = len;
        }

        public String String
        {
            get
            {
                return new String(this.Sen, this.WordOffset, this.Length);
            }
        }

        public override string ToString()
        {
            return String;
        }

        public int WordOffset
        {
            get { return offset; }
        }

        public int Length
        {
            get { return len; }
        }

        public char[] Sen
        {
            get { return sen; }
        }

        public int StartOffset
        {
            get { return startOffset + offset; }
        }

        public int EndOffset
        {
            get { return StartOffset + Length; }
        }

        public int Degree
        {
            get { return degree; }
            set { degree = value; }
        }

        public String Type
        {
            get { return type; }
            set { type = value; }
        }
    }
}
