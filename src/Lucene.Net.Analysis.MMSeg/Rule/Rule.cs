using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 过滤规则的抽象类
    /// </summary>
    public abstract class Rule
    {
        protected List<Chunk> chunks;


        public virtual void AddChunks(List<Chunk> chunks)
        {
            foreach (Chunk chunk in chunks)
            {
                AddChunk(chunk);
            }
        }

        public virtual void AddChunk(Chunk chunk)
        {
            chunks.Add(chunk);
        }

        /// <summary>
        /// 返回规则过虑后的结果
        /// </summary>
        /// <returns></returns>
        public List<Chunk> RemainChunks()
        {
            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                if (IsRemove(chunks[i]))
                    chunks.RemoveAt(i);
            }
            return chunks;
        }

        /// <summary>
        /// 判断chunk是否要删除
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        protected abstract bool IsRemove(Chunk chunk);

        public virtual void Reset()
        {
            chunks = new List<Chunk>();
        }
    }
}
