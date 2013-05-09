using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using Lucene.Net.Analysis.MMSeg;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 切分“字母和数”混在一起的过虑器。比如：mb991ch 切为 "mb 991 ch"
    /// </summary>
    public class CutLeterDigitFilter : TokenFilter
    {
        protected Queue<Token> tokenQueue = new Queue<Token>();

        ITermAttribute termAtt;
        IOffsetAttribute offsetAtt;
        ITypeAttribute typeAtt;
        Token reusableToken;

        public CutLeterDigitFilter(TokenStream input)
            : base(input)
        {
            reusableToken = new Token();
            termAtt = AddAttribute<ITermAttribute>();
            offsetAtt = AddAttribute<IOffsetAttribute>();
            typeAtt = AddAttribute<ITypeAttribute>();
        }

        public Token Next(Token reusableToken)
        {
            return NextToken(reusableToken);
        }

        Token NextToken(Token reusableToken)
        {
            Token nextToken = tokenQueue.Dequeue();
            if (nextToken != null) 
                return nextToken;
            nextToken = TokenUtils.NextToken(input, reusableToken);
            if (nextToken != null &&
                (Word.TYPE_LETTER_OR_DIGIT.Equals(nextToken.Type, StringComparison.CurrentCultureIgnoreCase)
                || Word.TYPE_DIGIT_OR_LETTER.Equals(nextToken.Type, StringComparison.CurrentCultureIgnoreCase)))
            {
                char[] buffer = nextToken.TermBuffer();
                int length = nextToken.TermLength();
                byte lastType = (byte)buffer[0];
                int termBufferOffset = 0;
                int termBufferLength = 0;
                byte type;
                for (int i = 0; i < length; i++)
                {
                    type = (byte)buffer[i];
                    if (type <= (byte)UnicodeCategory.ModifierLetter)
                    {
                        type = (byte)UnicodeCategory.LowercaseLetter;
                    }
                    //与上一次的不同
                    if (type != lastType)
                    {
                        AddToken(nextToken, termBufferOffset, termBufferLength, lastType);
                        termBufferOffset += termBufferLength;
                        termBufferLength = 0;
                        lastType = type;
                    }
                    termBufferLength++;
                }
                if (termBufferLength > 0) //最后一次
                {
                    AddToken(nextToken, termBufferOffset, termBufferLength, lastType);
                }
                nextToken = tokenQueue.Dequeue();
            }
            return nextToken;
        }

        void AddToken(Token oriToken, int termBufferOffset, int termBufferLength, byte type)
        {
            Token token = new Token(oriToken.TermBuffer(), termBufferOffset, termBufferLength,
                oriToken.StartOffset + termBufferOffset, oriToken.StartOffset + termBufferOffset + termBufferLength);
            if (type == (byte)UnicodeCategory.DecimalDigitNumber)
                token.Type = Word.TYPE_DIGIT;
            else
                token.Type = Word.TYPE_LETTER;
            tokenQueue.Enqueue(token);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            tokenQueue.Clear();
        }

        public override void Reset()
        {
            base.Reset();
            tokenQueue.Clear();
        }

        public override bool IncrementToken()
        {
            ClearAttributes();
            Token token = NextToken(reusableToken);
            if (tokenQueue != null)
            {
                termAtt.SetTermBuffer(token.TermBuffer(), 0, token.TermLength());
                offsetAtt.SetOffset(token.StartOffset, token.EndOffset);
                typeAtt.Type = token.Type;
                return true;
            }
            else
            {
                End();
                return false;
            }
        }

        public override void End()
        {
            try
            {
                Reset();
            }
            catch (IOException)
            { }
        }
    }
}
