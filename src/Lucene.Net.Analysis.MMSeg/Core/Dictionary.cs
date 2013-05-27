using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;

namespace Lucene.Net.Analysis.MMSeg
{
    /// <summary>
    /// 词典类. 词库目录单例模式,保存单字与其频率,还有词库.
    /// 有检测词典变更的接口，外部程序可以使用 <see cref="WordsFileIsChanged"/> 和 <see cref="reload"/> 来完成检测与加载的工作.
    /// </summary>
    public class Dictionary : IDisposable
    {
        /// <summary>
        /// 词库目录
        /// </summary>
        FileInfo dicPath;
        Dictionary<char, CharNode> dict;
        Dictionary<char, object> unit; //单个字的单位

        /// <summary>
        /// 记录word文件的最后修改时间
        /// </summary>
        Dictionary<string, long> wordsLastTime = null;
        long lastLoadTime = 0;

        static string defaultPath = null;

        static Dictionary<string, Dictionary> dics = new Dictionary<string, Dictionary>();

        static object m_syncOjbect = new object();

        Dictionary(FileInfo path)
        {
            init(path);
        }

        void init(FileInfo path)
        {
            dicPath = path;
            wordsLastTime = new Dictionary<string, long>();
            reload();
        }

        /// <summary>
        /// 获取wordsXXX.dic文件
        /// </summary>
        /// <returns></returns>
        string[] listWordsFiles()
        {
            if (string.IsNullOrEmpty(defaultPath)) return new string[] { };
            return Directory.GetFiles(defaultPath, "words*.dic");
        }

        void addLastTime(FileInfo file)
        {
            if (!file.Exists) return;
            lock (m_syncOjbect)
            {
                if (wordsLastTime.ContainsKey(file.ToString()))
                {
                    wordsLastTime[file.ToString()] = file.LastWriteTime.ToFileTime();
                }
                else
                {
                    wordsLastTime.Add(file.ToString(), file.LastWriteTime.ToFileTime());
                }
            }
        }

        Dictionary<char, CharNode> loadDic(FileInfo wordsPath)
        {
            string path = wordsPath.FullName;
            string[] data = null;
            if(!wordsPath.Exists)
                wordsPath = new FileInfo(string.Format("{0}\\data\\chars.dic",wordsPath.FullName));
            if (wordsPath.Exists)
            {
                data = File.ReadAllLines(wordsPath.FullName);
                addLastTime(wordsPath);
            }
            else
            {
                data = getLinesFromResources("chars.dic");
            }
            Dictionary<char,CharNode> dic = new Dictionary<char,CharNode>();
            int lineNum = 0;
            long s = now;
            long ss = s;
            lineNum = load(data, dic);
            Log.Info("chars loaded time={0}ms,line={1},on file={2}", now - s, lineNum, wordsPath);

            //try load words.dic
            string[] wordsDicIn = getLinesFromResources("words.dic");
            if (wordsDicIn != null)
            {
                loadWord(wordsDicIn, dic);
            }

            string[] words = listWordsFiles();
            if (words != null)
            {  
                foreach (string wordsFile in words)
                {
                    if (!File.Exists(wordsFile)) continue;
                    loadWord(File.ReadAllLines(wordsFile), dic);
                    addLastTime(new FileInfo(wordsFile));
                }
            }

            Log.Info("load all dic user time={0}ms", now - ss);

            return dic;
        }

        void loadWord(string[] buffers, Dictionary<char, CharNode> dic)
        {
            long s = now;
            int lineNum = WordsLoading(buffers, dic);
            Log.Info("words loaded time={0}ms,line={1},on file=words.dic", now - s, lineNum);
        }

        Dictionary<char,object> loadUnit(FileInfo path)
        {
            string[] lines = null;
            if (path.Exists)
            {
                lines = File.ReadAllLines(path.FullName);
                addLastTime(path);
            }
            else
            {
                lines = getLinesFromResources("units.dic");
            }
            Dictionary<char, object> unit = new Dictionary<char, object>();
            long s = now;
            int lineNum = 0;
           
                foreach (string line in lines)
                {
                    if (line.Length != 1) continue;
                    if (!unit.ContainsKey(line[0]))
                        unit.Add(line[0], typeof(Dictionary));
                    ++lineNum;
                }
            
            Log.Info("unit loaded time={0}ms,line={1},on file={2}", now - s, lineNum, path);
            return unit;
        }

        #region IDisposable 成员

        public void Dispose()
        {
            destroy();
        }

        /// <summary>
        /// 销毁,释放资源
        /// </summary>
        void destroy()
        {
            dicPath = null;
            dict.Clear();
            dict = null;
            unit.Clear();
            unit = null;
        }

        #endregion

        /// <summary>
        /// 词典目录
        /// </summary>
        /// <returns></returns>
        public static Dictionary getInstance()
        {
            return getInstance(getDefaultPath());
        }

        public static Dictionary getInstance(FileInfo path)
        {
            Dictionary dic = null;
            string key = path.ToString();
            lock (m_syncOjbect)
            {
                if (dics.ContainsKey(key))
                {
                    dic = dics[key];
                }
                else
                {
                    dic = new Dictionary(path);
                    dics.Add(key, dic);
                }
            }
            return dic;
        }

        /// <summary>
        /// 词典目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary getInstance(string path)
        {
            return getInstance(new FileInfo(path));
        }

        /// <summary>
        /// 获取默认路径
        /// </summary>
        /// <returns></returns>
        public static string getDefaultPath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// 从单例缓存中移除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary clear(FileInfo path)
        {
            Dictionary result = null;
            string key = path.ToString();
            lock (m_syncOjbect)
            {
                if (dics.ContainsKey(key))
                {
                    result = dics[key];
                    dics.Remove(key);
                }
            }
            return result;
        }

        public static Dictionary clear(string path)
        {
           return clear(new FileInfo(path));
        }

        /// <summary>
        /// 词典文件是否有修改过
        /// </summary>
        public bool WordsFileIsChanged
        {
            get
            {
                //检查是否有修改的文件,包括删除的
                foreach (KeyValuePair<string, long> key in wordsLastTime)
                {
                    FileInfo info = new FileInfo(key.Key);
                    if (!info.Exists) return true;
                    if (info.LastAccessTime.ToFileTime() != key.Value)
                        return true;
                }
                //检查是否有新文件
                FileInfo fi;
                foreach (string file in listWordsFiles())
                {
                    //有新词典文件
                    fi = new FileInfo(file);
                    if (!wordsLastTime.ContainsKey(file))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 全部加载词库,没有成功加载会回滚
        /// 注意：重新加载时，务必有两倍的词库树结构的内存，默认词库是 50M/个 左右。否则抛出 OOM。
        /// </summary>
        /// <returns></returns>
        public bool reload()
        {
            Dictionary<string, long> oldWordsLastTime = new Dictionary<string, long>(wordsLastTime);
            Dictionary<char, CharNode> oldDict = dict;
            Dictionary<char, object> oldUnit = unit;

            try
            {
                wordsLastTime.Clear();
                dict = loadDic(dicPath);
                unit = loadUnit(dicPath);
                lastLoadTime = now;
            }
            catch (Exception ex)
            {
                //rollback
                foreach (KeyValuePair<string, long> key in oldWordsLastTime)
                {
                    wordsLastTime.Add(key.Key, key.Value);
                }
                dict = oldDict;
                unit = oldUnit;
                Log.Info("reload dic error! dic={0},and rollbacked.{1}", dicPath, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// word能否在词库里找到
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool match(string word)
        {
            if (word == null || word.Length < 2) return false;
            if (!dict.ContainsKey(word[0])) return false;
            return search(dict[word[0]], word.ToCharArray(), 0, word.Length - 1) >= 0;
        }

        public CharNode head(char ch)
        {
            if (dict.ContainsKey(ch))
                return dict[ch];
            return null;
        }

        /// <summary>
        /// sen[offset]后tailLen长的词是否存在
        /// </summary>
        /// <param name="node"></param>
        /// <param name="sen"></param>
        /// <param name="offset"></param>
        /// <param name="tailLen"></param>
        /// <returns></returns>
        public int search(CharNode node, char[] sen, int offset, int tailLen)
        {
            if (node != null)
                return node.IndexOf(sen, offset, tailLen);
            return -1;
        }

        public int maxMatch(char[] sen, int offset)
        {
            CharNode node = null;
            if (dict.ContainsKey(sen[offset]))
                node = dict[sen[offset]];
            return maxMatch(node,sen,offset);
        }

        public int maxMatch(CharNode node, char[] sen, int offset)
        {
            if (node != null)
                return node.MaxMatch(sen, offset + 1);
            return 0;
        }

        public List<int> maxMatch(CharNode node, List<int> tailLens, char[] sen, int offset)
        {
            tailLens.Clear();
            tailLens.Add(0);
            if (node != null)
                return node.MaxMatch(tailLens, sen, offset + 1);
            return tailLens;
        }

        public bool isUnit(char ch)
        {
            return unit.ContainsKey(ch);
        }

        public Dictionary<char, CharNode> getDict
        {
            get { return dict; }
        }

        public FileInfo getDicPath { get { return dicPath; } }

        /// <summary>
        /// 最后加载词库的时间
        /// </summary>
        public long getLastLoadTime { get { return lastLoadTime; } }

        static long now
        {
            get { return DateTime.Now.Ticks; }
        }

        /// <summary>
        /// 取得str除去第一个char的部分
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static char[] tail(String str)
        {
            char[] cs = new char[str.Length - 1];
            char[] source = str.ToCharArray();
            Array.Copy(source, 1, cs, 0, cs.Length);
            return cs;
        }



        static string[] getLinesFromResources(string name)
        {
            string[] result = null;
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;
            //string[] names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            using (var textStream = assembly.GetManifestResourceStream(assemblyName + ".Resources." + name))
            {
                //var textStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                if (textStream != null)
                {
                    List<string> lines = new List<string>();
                    using (var reader = new StreamReader(textStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                    result = lines.ToArray();
                }
                else
                    result = new string[] { };
            }
            return result;
        }

        /// <summary>
        /// 文件总行数
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        static int load(string[] buffers, Dictionary<char, CharNode> dic)
        {
            if (buffers == null) return 0;
            int n = 0;
            string[] w = null;
            CharNode cn = null;
            foreach (string line in buffers)
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;
                cn = new CharNode();
                w = line.Split(' ');
                if (w.Length == 2)
                {
                    try
                    {
                        //字频计算出自由度
                        cn.Freq = (int)(Math.Log(Int32.Parse(w[1])) * 100);
                    }
                    catch
                    {
                    }
                }

                if (!dic.ContainsKey(w[0][0]))
                {

                    dic.Add(w[0][0], cn);
                }
                else
                    dic[w[0][0]] = cn;
                ++n;
            }
            return n;
        }

        static int WordsLoading(string[] buffers, Dictionary<char, CharNode> dic)
        {
            CharNode cn = null;
            int count = 0;
            foreach (string line in buffers)
            {
                if(line.Length < 2) continue;
                cn = null;
                if (dic.ContainsKey(line[0]))
                    cn = dic[line[0]];
                if (cn == null)
                {
                    cn = new CharNode();
                    dic.Add(line[0], cn);
                }
                ++count;
                cn.AddWordTail(tail(line));
            }
            return count;
        }
    }
}
