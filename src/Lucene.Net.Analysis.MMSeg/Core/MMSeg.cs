using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// Reader流的分词（有字母、数字等），析出中文（其实是CJK）成句子,再对mmseg算法分词
    /// 非线程安全
    /// </summary>
    public class MMSeg
    {
        PushbackReader reader;
        Seg seg;

        StringBuilder bufSentence = new StringBuilder(256);
        Sentence currentSentence;

        /// <summary>
        /// word缓存,因为有chunk分析三个以上
        /// </summary>
        Queue<Word> bufWord;

        public MMSeg(TextReader input, Seg seg)
        {
            this.seg = seg;
            reset(input);
        }

        int readedIdx = 0;

        public void reset(TextReader input)
        {
            this.reader = new PushbackReader(input);
            currentSentence = null;
            bufWord = new Queue<Word>();
            bufSentence.Length = 0;
            readedIdx = -1;
        }

        int ReadNext()
        {
            int d = reader.Read();
            if (d > -1)
            {
                readedIdx++;
                d = (int)char.ToLower((char)d);
            }
            return d;
        }

        void PushBack(int data)
        {
            readedIdx--;
            reader.Unread(data);
        }


        public Word Next()
        {
            Word word = null;
            if (bufWord.Count > 0)
                word = bufWord.Dequeue();

            if (word == null)
            {
                bufSentence.Length = 0;
                int data = -1;
                bool read = true;
                while (read && (data = ReadNext()) != -1)
                {
                    read = false;
                    UnicodeCategory type = char.GetUnicodeCategory((char)data);
                    #region 条件检测
                    switch (type)
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                        case UnicodeCategory.ModifierLetter:
                            #region exec digit or letter
                            /*
                             * 1. 0x410-0x44f -> А-я	//俄文   
                             * 2. 0x391-0x3a9 -> Α-Ω	//希腊大写
                             * 3. 0x3b1-0x3c9 -> α-ω	//希腊小写
                             * 
                             */
                            data = ToAscii(data);
                            NationLetter nl = GetNation(data);
                            if (nl == NationLetter.UNKNOW)
                            {
                                read = true;
                                break;
                            }
                            string wordType = Word.TYPE_LETTER;
                            bufSentence.Append((char)data);

                            switch (nl)
                            {
                                case NationLetter.EN:
                                    //字母后面的数字,如:VH049PA
                                    ReadCharByAsciiOrDigit rcad = new ReadCharByAsciiOrDigit();
                                    ReadChars(bufSentence, rcad);
                                    if (rcad.HasDigit)
                                        wordType = Word.TYPE_LETTER_OR_DIGIT;
                                    break;
                                case NationLetter.RA:
                                    ReadChars(bufSentence, new ReadCharByRussia());
                                    break;
                                case NationLetter.GE:
                                    ReadChars(bufSentence, new ReadCharByGreece());
                                    break;
                            }
                            bufWord.Enqueue(CreateWord(bufSentence, wordType));
                            bufSentence.Length = 0;
                            #endregion
                            break;

                        case UnicodeCategory.OtherLetter:
                            /*
                             * 1. 0x3041-0x30f6 -> ぁ-ヶ	    //日文(平|片)假名
                             * 2. 0x3105-0x3129 -> ㄅ-ㄩ	//注意符号
                             */
                            bufSentence.Append((char)data);
                            ReadChars(bufSentence, new ReadCharByType(UnicodeCategory.OtherLetter));

                            currentSentence = CreateSentence(bufSentence);
                            bufSentence.Length = 0;
                            break;

                        case UnicodeCategory.DecimalDigitNumber:
                            #region decimalDigitNumber

                            bufSentence.Append((char)ToAscii(data));
                            //读后面的数字,AsciiLetterOr
                            ReadChars(bufSentence, new ReadCharDigit());
                            wordType = Word.TYPE_DIGIT;
                            int d = ReadNext();
                            if (d > -1)
                            {
                                if (seg.IsUnit(d))
                                {
                                    //单位,如时间
                                    bufWord.Enqueue(CreateWord(bufSentence, StartIdx(bufSentence) - 1, Word.TYPE_DIGIT));
                                    bufSentence.Length = 0;
                                    bufSentence.Append((char)d);
                                    wordType = Word.TYPE_WORD;
                                }
                                else
                                {
                                    //后面可能是字母和数字
                                    PushBack(d);
                                    if (ReadChars(bufSentence, new ReadCharByAsciiOrDigit()) > 0)
                                    {
                                        wordType = Word.TYPE_DIGIT_OR_LETTER;
                                    }
                                }
                            }

                            bufWord.Enqueue(CreateWord(bufSentence, wordType));
                            bufSentence.Length = 0;
                            #endregion
                            break;

                        case UnicodeCategory.LetterNumber:
                            //ⅠⅡⅢ 单分
                            bufSentence.Append((char)data);
                            ReadChars(bufSentence, new ReadCharByType(UnicodeCategory.LetterNumber));
                            int startIdx = StartIdx(bufSentence);
                            for (int i = 0; i < bufSentence.Length; i++)
                            {
                                bufWord.Enqueue(new Word(new char[] { bufSentence[i] }, startIdx++, Word.TYPE_LETTER_NUMBER));
                            }
                            bufSentence.Length = 0;
                            break;
                        case UnicodeCategory.OtherNumber:
                            //①⑩㈠㈩⒈⒑⒒⒛⑴⑽⑾⒇ 连着用
                            bufSentence.Append((char)data);
                            ReadChars(bufSentence, new ReadCharByType(UnicodeCategory.OtherNumber));
                            bufWord.Enqueue(CreateWord(bufSentence, Word.TYPE_OTHER_NUMBER));
                            bufSentence.Length = 0;
                            break;
                        default:
                            //其它认为无效字符
                            read = true;
                            break;
                    }
                    #endregion
                }
                //中文分词
                if (currentSentence != null)
                {
                    Chunk chunk = null;
                    do
                    {
                        chunk = seg.Segment(currentSentence);
                        for (int i = 0; i < chunk.Count; i++)
                        {
                            bufWord.Enqueue(chunk.Words[i]);
                        }
                    } while (!currentSentence.IsFinish);
                    currentSentence = null;
                }
                if (bufWord.Count > 0)
                    word = bufWord.Dequeue();
            }
            return word;
        }

        /// <summary>
        /// 读取下一串指定类型字符.
        /// </summary>
        abstract class ReadChar
        {
            public abstract bool IsRead(int codePoint);
            public virtual int Transform(int codePoint)
            {
                return codePoint;
            }
        }

        /// <summary>
        /// 读取下一串指定类型的字符放到bufSentence中.
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="readChar"></param>
        /// <returns></returns>
        int ReadChars(StringBuilder buff, ReadChar readChar)
        {
            int num = 0;
            int data = -1;
            while ((data = ReadNext()) != -1)
            {
                int d = readChar.Transform(data);
                if (readChar.IsRead(d))
                {
                    buff.Append((char)d);
                    num++;
                }
                else
                {
                    //不是数字回压,要下一步操作
                    PushBack(data);
                    break;
                }
            }
            return num;
        }


        /// <summary>
        /// 读取数字
        /// </summary>
        class ReadCharDigit : ReadChar
        {
            public override bool IsRead(int codePoint)
            {
                return char.IsDigit((char)codePoint);
            }

            public override int Transform(int codePoint)
            {
                return ToAscii(codePoint);
            }
        }

        /// <summary>
        /// 读取字母或数字
        /// </summary>
        class ReadCharByAsciiOrDigit : ReadCharDigit
        {
            bool _hasDigit = false;

            public override bool IsRead(int codePoint)
            {
                bool iRead = base.IsRead(codePoint);
                _hasDigit |= iRead;
                return IsAsciiLetter(codePoint) || iRead;
            }

            public bool HasDigit
            {
                get { return _hasDigit; }
            }
        }

        /// <summary>
        /// 读取字母
        /// </summary>
        class ReadCharByAscii : ReadCharDigit
        {
            public override bool IsRead(int codePoint)
            {
                return IsAsciiLetter(codePoint);
            }
        }

        class ReadCharByRussia : ReadCharDigit
        {
            public override bool IsRead(int codePoint)
            {
                return IsRussiaLetter(codePoint);
            }
        }

        /// <summary>
        /// 读取希腊语
        /// </summary>
        class ReadCharByGreece : ReadCharDigit
        {
            public override bool IsRead(int codePoint)
            {
                return IsGreeceLetter(codePoint);
            }
        }

        class ReadCharByType : ReadChar
        {
            UnicodeCategory charType;

            public ReadCharByType(UnicodeCategory charType)
            {
                this.charType = charType;
            }

            public override bool IsRead(int codePoint)
            {
                System.Globalization.UnicodeCategory ci = char.GetUnicodeCategory((char)codePoint);
                return ci == charType;
            }
        }

        Word CreateWord(StringBuilder buff, string type)
        {
            return new Word(ToChars(buff), StartIdx(buff), type);
        }

        Word CreateWord(StringBuilder buff, int startIdx, string type)
        {
            return new Word(ToChars(buff), startIdx, type);
        }

        Sentence CreateSentence(StringBuilder buff)
        {
            return new Sentence(ToChars(buff), StartIdx(buff));
        }

        /// <summary>
        /// 取得bufSentence的第一个字符在整个文本中的位置
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        int StartIdx(StringBuilder buff)
        {
            return readedIdx - buff.Length + 1;
        }

        /// <summary>
        /// 从StringBuilder中复制出char[]
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        static char[] ToChars(StringBuilder buff)
        {
            char[] chs = new char[buff.Length];
            buff.CopyTo(0, chs, 0, chs.Length);
            return chs;
        }

        /// <summary>
        /// 双角转单角
        /// </summary>
        /// <param name="codePoint"></param>
        /// <returns></returns>
        static int ToAscii(int codePoint)
        {
            if ((codePoint >= 65296 && codePoint <= 65305)	//０-９
                    || (codePoint >= 65313 && codePoint <= 65338)	//Ａ-Ｚ
                    || (codePoint >= 65345 && codePoint <= 65370)	//ａ-ｚ
                    )
            {
                codePoint -= 65248;
            }
            return codePoint;
        }

        private static bool IsAsciiLetter(int codePoint)
        {
            return (codePoint >= 'A' && codePoint <= 'Z') || (codePoint >= 'a' && codePoint <= 'z');
        }

        private static bool IsRussiaLetter(int codePoint)
        {
            return (codePoint >= 'А' && codePoint <= 'я') || codePoint == 'Ё' || codePoint == 'ё';
        }

        private static bool IsGreeceLetter(int codePoint)
        {
            return (codePoint >= 'Α' && codePoint <= 'Ω') || (codePoint >= 'α' && codePoint <= 'ω');
        }

         /**
         * EN -> 英语
         * RA -> 俄语
         * GE -> 希腊
         * 
         */
        enum NationLetter { EN, RA, GE, UNKNOW }

        NationLetter GetNation(int codePoint)
        {
            if (IsAsciiLetter(codePoint))
                return NationLetter.EN;
            if (IsRussiaLetter(codePoint))
                return NationLetter.RA;
            if (IsGreeceLetter(codePoint))
                return NationLetter.GE;
            return NationLetter.UNKNOW;
        }



    }
}
