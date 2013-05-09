using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 所有词都记录在第一个字的节点下
    /// </summary>
    public class CharNode
    {
        int freq = -1; //Degree of Morphemic Freedom of One-Character, 单字才需要
        int maxLen = 0; //wordTail的最长

        KeyTree ktWordTails = new KeyTree();
        int wordNum = 0;

        public void AddWordTail(char[] wordTail)
        {
            ktWordTails.Add(wordTail);
            wordNum++;
            if (wordTail.Length > maxLen)
            {
                maxLen += wordTail.Length;
            }
        }

        public int Freq
        {
            get { return freq; }
            set { freq = value; }
        }

        public int WordNum
        {
            get { return wordNum; }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sen">句子,一串文本</param>
        /// <param name="offset">词在句子中的位置</param>
        /// <param name="tailLen">词尾的长度,实际是去掉词的长度</param>
        /// <returns></returns>
        public int IndexOf(char[] sen, int offset, int tailLen)
        {
            return ktWordTails.Match(sen, offset + 1, tailLen) ? 1 : -1;
        }

        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="sen">句子,一串文本</param>
        /// <param name="wordTailOffset">词在句子中的位置,实际是offset后面的开始找</param>
        /// <returns></returns>
        public int MaxMatch(char[] sen, int wordTailOffset)
        {
            return ktWordTails.MaxMatch(sen, wordTailOffset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tailLens"></param>
        /// <param name="sen"></param>
        /// <param name="wordTailOffset"></param>
        /// <returns>至少返回一个包括 0的int</returns>
        public List<int> MaxMatch(List<int> tailLens, char[] sen, int wordTailOffset)
        {
            return ktWordTails.MaxMatch(tailLens, sen, wordTailOffset);
        }

        public int MaxLen { 
            get { return maxLen; }
            set { maxLen = value; }
        }
    }

    public class KeyTree
    {
        TreeNode head = new TreeNode(' ');

        public void Add(char[] w)
        {
            if (w.Length < 1) 
                return;
            TreeNode p = head;
            for (int i = 0; i < w.Length; i++)
            {
                TreeNode n = p.SubNode(w[i]);
                if (n == null)
                {
                    n = new TreeNode(w[i]);
                    p.Born(w[i], n);
                }
                p = n;
            }
            p.IsAlsoLeaf = true;
        }

        /// <summary>
        /// 返回匹配最长词的长度,没有找到返回0.
        /// </summary>
        /// <param name="sen"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int MaxMatch(char[] sen, int offset)
        {
            int idx = offset - 1;
            TreeNode node = head;
            for (int i = offset; i < sen.Length; i++)
            {
                node = node.SubNode(sen[i]);
                if (node == null)
                    break;
                if (node.IsAlsoLeaf)
                    idx = i;
            }
            return idx - offset + 1;
        }

        public List<int> MaxMatch(List<int> tailLens, char[] sen, int offset)
        {
            TreeNode node = head;
            for (int i = offset; i < sen.Length; i++)
            {
                node = node.SubNode(sen[i]);
                if (node == null)
                    break;
                if (node.IsAlsoLeaf)
                    tailLens.Add(i - offset + 1);
            }
            return tailLens;
        }

        public bool Match(char[] sen, int offset, int len)
        {
            TreeNode node = head;
            for (int i = 0; i < len; i++)
            {
                node = node.SubNode(sen[offset + i]);
                if (node == null) 
                    return false;
            }
            return node.IsAlsoLeaf;
        }
    }

    public class TreeNode
    {
        char key;
        Dictionary<char, TreeNode> subNodes;
        bool alsoLeaf;

        public TreeNode(char key)
        {
            this.key = key;
            subNodes = new Dictionary<char, TreeNode>();
        }

        public void Born(char k, TreeNode sub)
        {
            subNodes[k] =sub;
        }

        public TreeNode SubNode(char k)
        {
            TreeNode node = null;
            subNodes.TryGetValue(k, out node); 
            return node;
        }

        public bool IsAlsoLeaf
        {
            get { return alsoLeaf; }
            set { alsoLeaf = value; }
        }
    }
}
