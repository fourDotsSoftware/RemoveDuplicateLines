using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace RemoveDuplicateLines
{ 
    class ArgsHelper
    {        
        public static bool ExamineArgs(string[] args)
        {
            if (args.Length == 0) return true;

            //MessageBox.Show(args[0]);
            Module.args = args;

            try
            {
                if (args[0].ToLower().Trim().StartsWith("-tempfile:"))
                {                                       

                    string tempfile = GetParameter(args[0]);

                    //MessageBox.Show(tempfile);

                    using (StreamReader sr = new StreamReader(tempfile, Encoding.Unicode))
                    {
                        string scont = sr.ReadToEnd();

                        //args = scont.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        args = SplitArguments(scont);
                        Module.args = args;

                        // MessageBox.Show(scont);
                    }
                }/*
                else if (args.Length>0 && (System.IO.File.Exists(Module.args[0]) || System.IO.Directory.Exists(Module.args[0])))
                {

                }*/
                else
                {
                    Module.IsCommandLine = true;

                    frmBatch fb = new frmBatch();

                    frmBatch.Instance.SilentAdd = true;

                    RemoveDuplicateLinesHelper.ForBatch = true;

                    for (int k = 0; k < Module.args.Length; k++)
                    {
                        if (System.IO.File.Exists(Module.args[k]))
                        {
                            frmBatch.Instance.AddFile(Module.args[k]);
                        }
                        else if (System.IO.Directory.Exists(Module.args[k]))
                        {
                            frmBatch.Instance.AddFolder(Module.args[k]);
                        }
                        else if (Module.args[k].ToLower().StartsWith("/sortmode:") ||
                    Module.args[k].ToLower().StartsWith("-sortmode:"))
                        {
                            Properties.Settings.Default.SortMode = int.Parse(GetParameter(Module.args[k]));
                        }
                        else if (Module.args[k].ToLower().StartsWith("/outfolder:") ||
                Module.args[k].ToLower().StartsWith("-outfolder:"))
                        {
                            frmBatch.Instance.cmbOutputDirText = GetParameter(Module.args[k]);
                        }
                        else if (Module.args[k].ToLower().StartsWith("/casesensitive") ||
                Module.args[k].ToLower().StartsWith("-casesensitive"))
                        {
                            Properties.Settings.Default.CaseSensitiveStringComparison = true;
                                
                        }
                        else if (Module.args[k].ToLower().StartsWith("/words") ||
                    Module.args[k].ToLower().StartsWith("-words"))
                        {
                            Properties.Settings.Default.FindWords = true;
                        }
                        else if (Module.args[k].ToLower().StartsWith("/list:") ||
                    Module.args[k].ToLower().StartsWith("-list:"))
                        {
                            frmBatch.Instance.ImportList(GetParameter(Module.args[k]));
                        }                        
                        else if (Module.args[k].ToLower() == "/h" ||
                        Module.args[k].ToLower() == "-h" ||
                        Module.args[k].ToLower() == "-?" ||
                        Module.args[k].ToLower() == "/?")
                        {
                            ShowCommandUsage();
                            return true;
                        }
                    }                                                                                                                       
                }
            }
            catch (Exception ex)
            {
                Module.ShowError("Error could not parse Arguments !", ex.ToString());
                return false;
            }


            return true;
        }

        private static string GetParameter(string arg)
        {
            int spos = arg.IndexOf(":");
            if (spos == arg.Length - 1) return "";
            else
            {
                string str=arg.Substring(spos + 1);

                if ((str.StartsWith("\"") && str.EndsWith("\"")) ||
                    (str.StartsWith("'") && str.EndsWith("'")))
                {
                    if (str.Length > 2)
                    {
                        str = str.Substring(1, str.Length - 2);
                    }
                    else
                    {
                        str = "";
                    }
                }

                return str;
            }
        }

        public static string[] SplitArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inSingleQuote = false;
            bool inDoubleQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    parmChars[index] = '\n';
                }
                if (parmChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    parmChars[index] = '\n';
                }
                if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void ShowCommandUsage()
        {
            string msg = GetCommandUsage();

            Module.ShowMessage(msg);

            Environment.Exit(0);
        }

        public static string GetCommandUsage()
        {
            string msg = "Remove duplicate lines or words from Text, RTF, Word Files.\n\n" +
            "RemoveDuplicateLines.exe [[file|directory]\n]" +
            "[/sortmode:SORT_MODE]\n"+
            "[/casesensitive]\n"+
            "[/words]\n"+
            "[/list:LIST_FILE]\n"+
            "[/outfolder:OUTPUT FOLDER PATH]\n"+
            "[/?]\n\n\n" +            
            "file : one or more files to be processed.\n" +
            "directory : one or more directories containing files to be processed.\n" +
            "/sortmode : sort mode. 0 do not sort, 1 sort ascending, 2 sort descending\n"+
            "/casesensitive : case sensitive\n"+
            "/words : remove duplicate words not lines\n"+
            "/list : import files from list. LIST_FILE : list filepath\n"+
            "/outfolder : output folder path\n" +
            "/? : show help\n";

            return msg;
        }

        public static bool IsFromWindowsExplorer
        {
            get
            {
                if (Module.IsFromWindowsExplorer) return true;

                // new
                if (Module.args.Length > 0 && (Module.args[0].ToLower().Trim().Contains("-tempfile:")
                    || System.IO.File.Exists(Module.args[0]) || System.IO.Directory.Exists(Module.args[0])))
                {
                    Module.IsFromWindowsExplorer = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsFromCommandLine
        {
            get
            {
                if (Module.args == null || Module.args.Length == 0)
                {
                    return false;
                }
                /*
                if (ArgsHelper.IsFromWindowsExplorer)
                {
                    Module.IsCommandLine = false;
                    return false;
                }*/
                else
                {
                    Module.IsCommandLine = true;
                    return true;
                }
            }
        }

        /*
        public static bool IsFromWindowsExplorer()
        {
            if (Module.args == null || Module.args.Length == 0)
            {
                return false;
            }

            for (int k = 0; k < Module.args.Length; k++)
            {
                if (Module.args[k] == "-visual")
                {
                    Module.IsFromWindowsExplorer = true;
                    return true;
                }
            }

            Module.IsFromWindowsExplorer = false;
            return false;
        }
        */

        public static void ExecuteCommandLine()
        {
            string err = "";
            bool finished = false;

            try
            {
                if (frmBatch.Instance.dt.Rows.Count == 0)
                {
                    err += "Please documents to remove duplicates !";
                    ShowCommandUsage();
                    Environment.Exit(0);
                    return;
                }

                try
                {
                    err += frmBatch.Instance.ExecuteCommandLine();
                    finished = true;
                }
                catch (Exception ex)
                {
                    err += ex.Message + "\r\n";
                }
            }
            finally
            {
                if (err == string.Empty && finished)
                {
                    Module.ShowMessage("Operation completed successfully !");
                }
                else
                {
                    Module.ShowMessage("An error occured !\n" + err);
                }
            }

            Environment.Exit(0);
        }                                        
    }

    public class ReadListsResult
    {
        public bool Success = true;
        public string err = "";
    }
}
