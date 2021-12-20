using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;

namespace RemoveDuplicateLines
{
    public class RemoveDuplicateLinesHelper
    {
        public int ThreadCount = 5;
        public List<System.ComponentModel.BackgroundWorker> bWorkers = new List<System.ComponentModel.BackgroundWorker>();
        //3public static MatchCollection macol = null;
        //4public static List<LineSorterLine> lst = null;

        public HashSortList lst = null;
        //3public static List<string> macol = new List<string>();
        //3public static bool[] macolSuc = null;        

        public static bool ForBatch = false;

        public string RemoveDuplicateWords(string txt, int sort)
        {
            ThreadCount = Environment.ProcessorCount;

            lst = new HashSortList();

            string line = "";

            System.Text.RegularExpressions.Regex regex=new Regex(@"\b(\w+?)\b",RegexOptions.Compiled);

            System.Text.RegularExpressions.MatchCollection macol=regex.Matches(txt);

            for (int k=0;k<macol.Count;k++)
            {
                LineSorterLine lr=new LineSorterLine(macol[k].Captures[0].Value,macol[k].Captures[0].Index);

                lst.Add(lr);
            }

            if (lst.Count == 0) return string.Empty;

            lst.HashSort();            

            LineSorterLine.SortMode = 3;

            lst.Sort();           

            int delchars=0;

            string newtxt=txt;

            for (int k=0;k<lst.Count;k++)
            {
                if (!lst[k].Include)
                {
                    newtxt=newtxt.Substring(0,lst[k].Index-delchars)+newtxt.Substring(lst[k].Index-delchars+lst[k].Value.Length);

                    delchars+=lst[k].Value.Length;
                }
            }

            return newtxt;
        }

        public string RemoveDuplicateLines(string txt, int sort)
        {
            return RemoveDuplicateLines(txt, sort, false);
        }

        public string RemoveDuplicateLines(string txt, int sort,bool forBatch)
        {
            ThreadCount = Environment.ProcessorCount;

            ForBatch = forBatch;

            //ThreadCount = 1;

            //3Regex regex = new Regex(@"[\s\S]*?(?:\\n|\\r\\n|\\n\\r)", RegexOptions.Multiline);            

            //3macol = regex.Matches(txt);

            //3macol.Clear();

            //4lst = new List<LineSorterLine>();
            lst = new HashSortList();

            string line = "";

            for (int k = 0; k < txt.Length; k++)
            {
                if (k < (txt.Length - 1) && txt[k] == '\r' && txt[k + 1] == '\n')
                {
                    line = line + txt[k] + txt[k + 1];
                    k = k + 1;

                    //3macol.Add(line);

                    lst.Add(new LineSorterLine(line, lst.Count - 1));

                    line = "";
                }
                else if (k < (txt.Length - 1) && txt[k] == '\r' && txt[k + 1] != '\n')
                {
                    line = line + txt[k];

                    //3macol.Add(line);

                    lst.Add(new LineSorterLine(line, lst.Count - 1));

                    line = "";
                }
                else if (k < (txt.Length - 1) && txt[k] == '\n' && txt[k + 1] == '\r')
                {
                    line = line + txt[k] + txt[k + 1];
                    k = k + 1;

                    //3macol.Add(line);\

                    lst.Add(new LineSorterLine(line, lst.Count - 1));

                    line = "";
                }
                else if (k < (txt.Length - 1) && txt[k] == '\n' && txt[k + 1] != '\r')
                {
                    line = line + txt[k];

                    //3macol.Add(line);

                    lst.Add(new LineSorterLine(line, lst.Count - 1));

                    line = "";
                }
                else
                {
                    line = line + txt[k];
                }
            }

            if (line != string.Empty)
            {
                //3macol.Add(line);

                lst.Add(new LineSorterLine(line, lst.Count - 1));
            }

            //3if (macol.Count == 0) return string.Empty;

            if (lst.Count == 0) return string.Empty;

            //3macolSuc = new bool[macol.Count];

            //3macolSuc = new bool[lst.Count];

            if (!forBatch)
            {
                frmMain.Instance.bwAction.ReportProgress(-1, (object)lst.Count);
            }

            lst.HashSort();            

            LineSorterLine.SortMode = 4;

            lst.Sort();

            /*
            bWorkers.Clear();

            for (int k = 0; k < ThreadCount; k++)
            {
                System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
                bw.DoWork += bw_DoWork;

                bWorkers.Add(bw);
            }
            */

            //3for (int k = 0; k < macol.Count; k++)            
            /*
            for (int k = 0; k < lst.Count; k++)
            {
                int len = lst[k].Length;

                for (int j = k + 1; j < lst.Count; j++)
                {
                    if (lst[j].Length != len)
                    {
                        break;
                    }

                    if (lst[k].Value.CompareTo(lst[j].Value)==0)
                    {
                        lst[j].Include = false;
                        k++;
                    }
                }
            */
            /*
            int j = 0;

            bool found = false;

            while (!found)
            {
                if (!bWorkers[j].IsBusy)
                {
                    found = true;

                    bWorkers[j].RunWorkerAsync(k);
                }
                else
                {
                    j = j + 1;

                    if (j >= bWorkers.Count)
                    {
                        j = 0;

                        System.Threading.Thread.Sleep(100);
                    }
                }
            }

            int m = k + 1;

            while (m<lst.Count && lst[m].Length == len)
            {
                k++;
                m++;

                if (k == lst.Count)
                {
                    break;
                }
            }
        }
        */
            /*
            bool done = false;

            while (!done)
            {
                done = true;

                for (int k = 0; k < bWorkers.Count; k++)
                {
                    if (bWorkers[k].IsBusy)
                    {
                        done = false;

                        break;
                    }
                }

                System.Threading.Thread.Sleep(300);
            }
            */

            StringBuilder sb = new StringBuilder();

            LineSorterLine.SortMode = Properties.Settings.Default.SortMode;

            if (Properties.Settings.Default.SortMode == 1
                || Properties.Settings.Default.SortMode == 2)
            {                
                lst.Sort();
            }
            else
            {
                LineSorterLine.SortMode = 3;

                lst.Sort();
            }

            for (int k = 0; k < lst.Count; k++)
            {
                if (lst[k].Include)
                {
                    sb.Append(lst[k].Value);
                }
            }

            return sb.ToString();
        }

        void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int kk = (int)e.Argument;

            int len = lst[kk].Length;            

            //aaaaa
            //aaaba
            //abaaa
            //aaaaa

            for (int j = kk; j < lst.Count; j++)
            {
                if (lst[j].Length != len)
                {
                    break;
                }
                else
                {
                    for (int n = j+1; n < lst.Count; n++)
                    {
                        if (lst[n] != lst[j])
                        {
                            break;
                        }
                   
                        if (lst[n].Length != len)
                        {
                            break;
                        }

                        if (n == j)
                        {
                            continue;
                        }

                        for (int m = 0; m < lst[n].Length; m++)
                        {
                            if (lst[n].Value[m] != lst[j].Value[m])
                            {
                                break;
                            }
                            else if (lst[n].Value[m] == lst[j].Value[m] && m == lst[n].Length - 1)
                            {
                                lst[n].Include = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public class HashTreeNode
    {
        public string Hash = "";

        public List<LineSorterLine> Lines = new List<LineSorterLine>();

        public Hashtable SubHashes = new Hashtable();        
    }

    public class HashSortList : List<LineSorterLine>
    {
        public System.Collections.Hashtable Hashes = new System.Collections.Hashtable();

        public void HashSort()
        {
            HashTreeNode hashnode = new HashTreeNode();

            /*
            RemoveDuplicateLinesHelper.ThreadCount = Environment.ProcessorCount;

            RemoveDuplicateLinesHelper.bWorkers.Clear();

            for (int k = 0; k < RemoveDuplicateLinesHelper.ThreadCount; k++)
            {
                RemoveDuplicateLinesHelper.bWorkers.Add(new System.ComponentModel.BackgroundWorker());

                RemoveDuplicateLinesHelper.bWorkers[k].DoWork += HashSortList_DoWork;
            }
            */

            for (int k = 0; k < this.Count; k++)
            {
                HashTreeNode currentHashNode = hashnode;
                LineSorterLine line=this[k];

                FindAndAddHashTreeNode("", ref line, ref hashnode);

                int mod = k % 10;

                if (!RemoveDuplicateLinesHelper.ForBatch)
                {
                    if (mod == 0)
                    {
                        frmMain.Instance.bwAction.ReportProgress(0, k);
                    }
                }

                /*
                List<object> lst = new List<object>();
                lst.Add(line);
                lst.Add(hashnode);

                int j = 0;

                bool found = false;

                while (!found)
                {
                    if (!RemoveDuplicateLinesHelper.bWorkers[j].IsBusy)
                    {
                        found = true;

                        RemoveDuplicateLinesHelper.bWorkers[j].RunWorkerAsync(lst);
                    }
                    else
                    {
                        j = j + 1;

                        if (j >= RemoveDuplicateLinesHelper.bWorkers.Count)
                        {
                            j = 0;

                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                */
            }

            if (!RemoveDuplicateLinesHelper.ForBatch)
            {
                frmMain.Instance.bwAction.ReportProgress(0, this.Count);
            }

            /*
            bool done = false;

            while (!done)
            {
                done = true;

                for (int k = 0; k < RemoveDuplicateLinesHelper.bWorkers.Count; k++)
                {
                    if (RemoveDuplicateLinesHelper.bWorkers[k].IsBusy)
                    {
                        done = false;

                        break;
                    }
                }

                System.Threading.Thread.Sleep(300);
            }
            */

            Hashes.Add("ROOT", hashnode);
        }

        void HashSortList_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            List<object> lst = (List<object>)e.Argument;

            LineSorterLine line = (LineSorterLine)lst[0];

            HashTreeNode hashnode = (HashTreeNode)lst[1];

            FindAndAddHashTreeNode("", ref line, ref hashnode);
        }

        private void FindAndAddHashTreeNode(string currentHash,ref LineSorterLine line,ref HashTreeNode parentHashTreeNode)
        {
            StringComparison str = Properties.Settings.Default.CaseSensitiveStringComparison ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            if (parentHashTreeNode.Lines.Count>0 && parentHashTreeNode.Lines[0].Value.Equals(line.Value,str)) // duplicate value
            {
                lock (line)
                {
                    line.Include = false;
                }

                lock (parentHashTreeNode)
                {
                    parentHashTreeNode.Lines.Add(line);
                }
            }
            else            
            {
                if (line.Value.Length >= currentHash.Length + 1)
                {
                    currentHash = line.Value.Substring(0, currentHash.Length + 1).ToLower();
                }
                else
                {
                    currentHash = line.Value.ToLower();
                }

                bool contains=false;

                lock (parentHashTreeNode)
                {
                    contains = parentHashTreeNode.SubHashes.ContainsKey(currentHash);

                    if (contains)
                    {
                        HashTreeNode hn = null;

                        lock (parentHashTreeNode)
                        {
                            foreach (DictionaryEntry entry in parentHashTreeNode.SubHashes)
                            {
                                if (entry.Key.Equals(currentHash))
                                {
                                    hn = (HashTreeNode)entry.Value;

                                    break;
                                }
                            }
                        }

                        FindAndAddHashTreeNode(currentHash, ref line, ref hn);
                    }
                    else
                    {
                        HashTreeNode hn = new HashTreeNode();
                        hn.Hash = currentHash;
                        hn.Lines.Add(line);


                        parentHashTreeNode.SubHashes.Add(currentHash, hn);

                    }
                }
            }
        }

        /*
        public void HashSort()
        {
            List<string> lst = new List<string>();

            for (int k = 0; k < this.Count; k++)
            {
                int len = this[k].Length;

                if (len == 0) continue;

                List<LineSorterLine> vals = new List<LineSorterLine>();

                string hash = this[k].Value[0].ToString();

                bool found = false;

                int m = 0;

                do
                {
                    List<LineSorterLine> vals2 = new List<LineSorterLine>();

                    string hash2 = hash + this[k].Value[m];

                    if (!Hashes.Contains(hash2))
                    {
                        for (int j = 0; j < this.Count; j++)
                        {
                            //if (j == k) continue;

                            if (this[k].Length != this[j].Length) continue;

                            if (this[j].Value.StartsWith(hash2))
                            {
                                vals2.Add(this[j]);

                                found = true;
                            }
                        }

                        if (found)
                        {
                            hash = hash2;
                            vals = vals2;
                        }
                    }
                }
                while (found && m<len);

                if (found)
                {
                    Hashes.Add(hash, vals);
                }
            }
        }*/
    }


    public class LineSorterLine : IComparable<LineSorterLine>
      {
        public string Value = "";
        public int Index = -1;
        public int Length = -1;
        public bool Include = true;

        public static int SortMode = -1;

        public LineSorterLine(string val,int index)
        {
            Value = val;
            Index = index;
            Length = Value.Length;
        }

        public int CompareTo(LineSorterLine as2)
        {
            if (SortMode == 1)
            {
                return this.Value.CompareTo(as2.Value);
            }
            else if (SortMode == 2)
            {
                return as2.Value.CompareTo(this.Value);
            }
            else if (SortMode == 3)
            {
                return this.Index.CompareTo(as2.Index);
            }
            else if (SortMode == 4)
            {
                int sort= this.Length.CompareTo(as2.Length);

                if (sort == 0)
                {
                    return this.Value.CompareTo(as2.Value);
                }
                else
                {
                    return sort;
                }
            }

            return 0;
        }
  
      }

}
