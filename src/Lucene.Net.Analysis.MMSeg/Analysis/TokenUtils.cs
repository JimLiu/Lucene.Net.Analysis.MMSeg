using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace Lucene.Net.Analysis.MMSeg
{
    public class TokenUtils
    {
        public static Token NextToken(TokenStream input, Token reusableToken)
        {
            if (input == null) 
                return null;
            if (!input.IncrementToken()) 
                return null;

            ITermAttribute termAtt = input.GetAttribute<ITermAttribute>();
            IOffsetAttribute offsetAtt = input.GetAttribute<IOffsetAttribute>();
            ITypeAttribute typeAtt = input.GetAttribute<ITypeAttribute>();

            if (reusableToken == null)
            {
                reusableToken = new Token();
            }
            reusableToken.Clear();

            if (termAtt != null)
                reusableToken.SetTermBuffer(termAtt.TermBuffer(), 0, termAtt.TermLength());

            if (offsetAtt != null)
            {
                reusableToken.StartOffset = offsetAtt.StartOffset;
                reusableToken.EndOffset = offsetAtt.EndOffset;
            }

            if (typeAtt != null)
                reusableToken.Type = typeAtt.Type;

            return reusableToken;
        }
    }
}
