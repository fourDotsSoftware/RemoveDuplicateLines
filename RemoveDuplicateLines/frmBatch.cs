using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace RemoveDuplicateLines
{
    public partial class frmBatch : RemoveDuplicateLines.CustomForm
    {
        public DataTable dt = new DataTable("table");

        public static frmBatch Instance = null;

        public bool SilentAdd = false;
        public string SilentAddErr = "";

        public bool OperationStopped = false;
        public bool OperationPaused = false;

        public string Err = "";

        private string sOutputDir = "";
        private bool bKeepBackup = false;

        public string FirstOutputDocument = "";

        public frmBatch()
        {
            InitializeComponent();

            dt.Columns.Add("filename");
            dt.Columns.Add("slideranges");
            dt.Columns.Add("sizekb");
            dt.Columns.Add("fullfilepath");
            dt.Columns.Add("filedate");
            dt.Columns.Add("rootfolder");

            dgFiles.AutoGenerateColumns = false;

            Instance = this;

            if (Module.IsCommandLine)
            {
                this.Visible = false;
                this.ShowInTaskbar = false;

                frmBatch_Load(null, null);
            }
        }

        private bool _IsDirty = false;

        private bool IsDirty
        {
            get { return _IsDirty; }

            set
            {
                _IsDirty = value;

                lblTotal.Text = TranslateHelper.Translate("Total") + " : " + dt.Rows.Count + " " + TranslateHelper.Translate("Documents");
            }
        }

        private void frmBatch_Load(object sender, EventArgs e)
        {
            dgFiles.DataSource = dt;

            RecentFilesHelper.FillMenuRecentFile();
            RecentFilesHelper.FillMenuRecentFolder();
            RecentFilesHelper.FillMenuRecentImportList();

            chkParentFolder.Checked = Properties.Settings.Default.KeepParentFolderPath;

            SetupOutputFolders();

            OutputFolderHelper.LoadOutputFolders();

            caseSensitiveStringComparisonToolStripMenuItem.Checked = Properties.Settings.Default.CaseSensitiveStringComparison;

            if (Properties.Settings.Default.SortMode == 0)
            {
                donotSortToolStripMenuItem.Checked = true;
            }
            else if (Properties.Settings.Default.SortMode == 1)
            {
                sortAscendingToolStripMenuItem.Checked = true;
            }
            else if (Properties.Settings.Default.SortMode == 2)
            {
                sortDescendingToolStripMenuItem.Checked = true;
            }

            findWordsNotLinesToolStripMenuItem.Checked = Properties.Settings.Default.FindWords;
        }

        private void tsdbAddFile_ButtonClick(object sender, EventArgs e)
        {
            openFileDialog1.Filter = Module.OpenFilesFilter;
            openFileDialog1.Multiselect = true;

            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SilentAddErr = "";

                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    for (int k = 0; k < openFileDialog1.FileNames.Length; k++)
                    {
                        AddFile(openFileDialog1.FileNames[k]);
                        RecentFilesHelper.AddRecentFile(openFileDialog1.FileNames[k]);
                    }
                }
                finally
                {
                    this.Cursor = null;

                    if (SilentAddErr != string.Empty)
                    {
                        frmError f = new frmError(TranslateHelper.Translate("Error"), SilentAddErr);
                        f.ShowDialog(this);
                    }
                }
            }
        }

        private void tsdbAddFile_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                SilentAddErr = "";

                AddFile(e.ClickedItem.Text);
                RecentFilesHelper.AddRecentFile(e.ClickedItem.Text);

            }
            finally
            {
                this.Cursor = null;

                if (SilentAddErr != string.Empty)
                {
                    frmError f = new frmError(TranslateHelper.Translate("Error"), SilentAddErr);
                    f.ShowDialog(this);
                }
            }
        }

        private void tsbRemove_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedCellCollection cells = dgFiles.SelectedCells;
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            for (int k = 0; k < cells.Count; k++)
            {
                if (rows.IndexOf(dgFiles.Rows[cells[k].RowIndex]) < 0)
                {
                    rows.Add(dgFiles.Rows[cells[k].RowIndex]);
                }
            }

            for (int k = 0; k < rows.Count; k++)
            {
                dgFiles.Rows.Remove(rows[k]);
            }

            IsDirty = true;
        }

        private void tsdbAddFolder_ButtonClick(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = "";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                SilentAddErr = "";

                AddFolder(folderBrowserDialog1.SelectedPath);
                RecentFilesHelper.AddRecentFolder(folderBrowserDialog1.SelectedPath);

                if (SilentAddErr != string.Empty)
                {
                    frmError f = new frmError(TranslateHelper.Translate("Error"), SilentAddErr);
                    f.ShowDialog(this);
                }
            }
        }

        private void tsdbAddFolder_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SilentAddErr = "";

            AddFolder(e.ClickedItem.Text, "");
            RecentFilesHelper.AddRecentFolder(e.ClickedItem.Text);

            if (SilentAddErr != string.Empty)
            {
                frmError f = new frmError(TranslateHelper.Translate("Error"), SilentAddErr);
                f.ShowDialog(this);
            }
        }

        public void ImportList(string listfilepath)
        {
            string curdir = Environment.CurrentDirectory;

            try
            {
                SilentAdd = true;
                using (StreamReader sr = new StreamReader(listfilepath, Encoding.Default, true))
                {
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("#"))
                        {
                            continue;
                        }

                        string filepath = line;
                        string password = "";

                        try
                        {
                            if (line.StartsWith("\""))
                            {
                                int epos = line.IndexOf("\"", 1);

                                if (epos > 0)
                                {
                                    filepath = line.Substring(1, epos - 1);
                                }
                            }
                            else if (line.StartsWith("'"))
                            {
                                int epos = line.IndexOf("'", 1);

                                if (epos > 0)
                                {
                                    filepath = line.Substring(1, epos - 1);
                                }
                            }

                            int compos = line.IndexOf(",");

                            if (compos > 0)
                            {
                                password = line.Substring(compos + 1);

                                if (!line.StartsWith("\"") && !line.StartsWith("'"))
                                {
                                    filepath = line.Substring(0, compos);
                                }

                                if ((password.StartsWith("\"") && password.EndsWith("\""))
                                    || (password.StartsWith("'") && password.EndsWith("'")))
                                {
                                    if (password.Length == 2)
                                    {
                                        password = "";
                                    }
                                    else
                                    {
                                        password = password.Substring(1, password.Length - 2);
                                    }
                                }

                            }
                        }
                        catch (Exception exq)
                        {
                            SilentAddErr += TranslateHelper.Translate("Error while processing List !") + " " + line + " " + exq.Message + "\r\n";
                        }

                        line = filepath;

                        Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(listfilepath);

                        line = System.IO.Path.GetFullPath(line);

                        if (System.IO.File.Exists(line))
                        {
                            AddFile(line, password);
                            /*
                            else
                            {
                                SilentAddErr += TranslateHelper.Translate("Error wrong file type !") + " " + line + "\r\n";
                            }*/
                        }
                        else if (System.IO.Directory.Exists(line))
                        {
                            AddFolder(line, password);
                        }
                        else
                        {
                            SilentAddErr += TranslateHelper.Translate("Error. File or Directory not found !") + " " + line + "\r\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SilentAddErr += TranslateHelper.Translate("Error could not read file !") + " " + ex.Message + "\r\n";
            }
            finally
            {
                Environment.CurrentDirectory = curdir;

                SilentAdd = false;
            }
        }

        private void tsdbImportList_ButtonClick(object sender, EventArgs e)
        {
            SilentAddErr = "";

            openFileDialog1.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImportList(openFileDialog1.FileName);
                RecentFilesHelper.ImportListRecent(openFileDialog1.FileName);

                if (SilentAddErr != string.Empty)
                {
                    frmMessage f = new frmMessage();
                    f.txtMsg.Text = SilentAddErr;
                    f.ShowDialog();

                }
            }
        }

        private void tsdbImportList_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SilentAddErr = "";

            ImportList(e.ClickedItem.Text);
            RecentFilesHelper.ImportListRecent(e.ClickedItem.Text);

            if (SilentAddErr != string.Empty)
            {
                frmMessage f = new frmMessage();
                f.txtMsg.Text = SilentAddErr;
                f.ShowDialog();

            }
        }

        public bool AddFile(string filepath)
        {
            return AddFile(filepath, "", "");
        }

        public bool AddFile(string filepath, string password)
        {
            return AddFile(filepath, password, "");
        }

        public bool AddFile(string filepath, string password, string rootfolder)
        {
            string ext = "*" + System.IO.Path.GetExtension(filepath).ToLower() + ";";

            /*
            if (Module.AcceptableMediaInputPattern.IndexOf(ext) < 0)
            {
                SilentAddErr += filepath + "\n\n" + TranslateHelper.Translate("Please add only Word Files !") + "\n\n";

                return false;
            }
            */

            DataRow dr = dt.NewRow();

            FileInfo fi = new FileInfo(filepath);

            long sizekb = fi.Length / 1024;
            dr["filename"] = fi.Name;
            dr["fullfilepath"] = filepath;
            dr["sizekb"] = sizekb.ToString() + "KB";
            dr["filedate"] = fi.LastWriteTime.ToString();
            dr["rootfolder"] = rootfolder;

            dt.Rows.Add(dr);

            if (dt.Rows.Count == 1)
            {
                string outfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filepath), "mergedDocument.docx");

                RecentFilesHelper.AddRecentOutputFile(outfile);
            }

            IsDirty = true;

            return true;
        }

        public void AddFolder(string folder_path)
        {
            AddFolder(folder_path, "");
        }

        public void AddFolder(string folder_path, string password)
        {
            string[] filez = null;

            if (!SilentAdd)
            {
                if (System.IO.Directory.GetDirectories(folder_path).Length > 0)
                {
                    DialogResult dres = Module.ShowQuestionDialog("Would you like to add also Subdirectories ?", TranslateHelper.Translate("Add Subdirectories ?"));

                    if (dres == DialogResult.Yes)
                    {
                        filez = System.IO.Directory.GetFiles(folder_path, "*.*", SearchOption.AllDirectories);
                    }
                    else
                    {
                        filez = System.IO.Directory.GetFiles(folder_path, "*.*", SearchOption.TopDirectoryOnly);
                    }
                }
                else
                {
                    filez = System.IO.Directory.GetFiles(folder_path, "*.*", SearchOption.TopDirectoryOnly);
                }
            }
            else
            {
                // silent add for import list
                filez = System.IO.Directory.GetFiles(folder_path, "*.*", SearchOption.AllDirectories);
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                for (int k = 0; k < filez.Length; k++)
                {
                    string filepath = filez[k];

                    //if (Module.IsWordDocument(filepath) || Module.IsPPDocument(filepath) || Module.IsExcelDocument(filepath))
                    //if (Module.Document(filepath))
                    if (true)
                    {
                        AddFile(filez[k], password, folder_path);
                    }
                }
            }
            finally
            {
                this.Cursor = null;
            }

        }

        #region Grid Context menu

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exploreToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyFullFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRowView drv = (DataRowView)dgFiles.CurrentRow.DataBoundItem;

            DataRow dr = drv.Row;

            string filepath = dr["fullfilepath"].ToString();

            Clipboard.Clear();

            Clipboard.SetText(filepath);
        }

        private void cmsFiles_Opening(object sender, CancelEventArgs e)
        {

        }
        #endregion

        #region Drag and Drop

        private void dgFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void dgFiles_DragOver(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void dgFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] filez = (string[])e.Data.GetData(DataFormats.FileDrop);

                for (int k = 0; k < filez.Length; k++)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        if (System.IO.File.Exists(filez[k]))
                        {
                            AddFile(filez[k]);
                        }
                        else if (System.IO.Directory.Exists(filez[k]))
                        {
                            AddFolder(filez[k]);
                        }
                    }
                    finally
                    {
                        this.Cursor = null;
                    }
                }
            }
        }

        #endregion

        private void cmsFiles_Opening_1(object sender, CancelEventArgs e)
        {
            Point p = dgFiles.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y));
            DataGridView.HitTestInfo hit = dgFiles.HitTest(p.X, p.Y);

            if (hit.Type == DataGridViewHitTestType.Cell)
            {
                dgFiles.CurrentCell = dgFiles.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
            }

            if (dgFiles.CurrentRow == null)
            {
                e.Cancel = true;
            }
        }

        private void chkParentFolder_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.KeepParentFolderPath = chkParentFolder.Checked;
        }

        private void btnChangeFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbr = new FolderBrowserDialog();

            if (cmbOutputDir.Text != string.Empty && System.IO.Directory.Exists(cmbOutputDir.Text))
            {
                fbr.SelectedPath = cmbOutputDir.Text;
            }

            if (fbr.ShowDialog() == DialogResult.OK)
            {
                OutputFolderHelper.SaveOutputFolder(fbr.SelectedPath);
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            dgFiles.EndEdit();

            if (dt.Rows.Count == 0)
            {
                Module.ShowMessage("Please add a Document first !");

            }
            else
            {
                string dirpath = "";
                string filepath = "";
                string outfilepath = "";

                if (dgFiles.SelectedCells.Count == 0)
                {
                    filepath = dgFiles.Rows[0].Cells["colFullfilepath"].Value.ToString();
                }
                else
                {
                    filepath = dgFiles.SelectedCells[0].OwningRow.Cells["colFullfilepath"].Value.ToString();
                }

                if (frmBatch.Instance.cmbOutputDir.Text.Trim() == TranslateHelper.Translate("Same Folder of Document"))
                {
                    dirpath = System.IO.Path.GetDirectoryName(filepath);                    

                }
                else if (frmBatch.Instance.cmbOutputDir.Text.ToString().StartsWith(TranslateHelper.Translate("Subfolder") + " : "))
                {
                    int subfolderspos = (TranslateHelper.Translate("Subfolder") + " : ").Length;
                    string subfolder = frmBatch.Instance.cmbOutputDir.Text.ToString().Substring(subfolderspos);

                    dirpath = System.IO.Path.GetDirectoryName(filepath) + "\\" + subfolder;
                }
                else if (frmBatch.Instance.cmbOutputDir.Text.Trim() == TranslateHelper.Translate("Overwrite Document"))
                {
                    dirpath = System.IO.Path.GetDirectoryName(filepath);                    
                }
                else
                {
                    dirpath = frmBatch.Instance.cmbOutputDir.Text;
                }

                if (!System.IO.Directory.Exists(dirpath))
                {
                    Module.ShowMessage(TranslateHelper.Translate("Folder does not exist !") + " " + dirpath);

                    return;
                }

                string args = string.Format("/e, /select, \"{0}\"", dirpath);

                ProcessStartInfo info = new ProcessStartInfo();

                info.FileName = "explorer";
                info.UseShellExecute = true;
                info.Arguments = args;

                Process.Start(info);
            }

        }

        public string cmbOutputDirText="";

        public string GetOutputFilepath(string filepath,string rootfolder)
        {
            string dirpath = "";

            string outfilepath="";

            if (cmbOutputDirText.Trim() == TranslateHelper.Translate("Same Folder of Document"))
            {
                dirpath = System.IO.Path.GetDirectoryName(filepath);

            }
            else if (cmbOutputDirText.ToString().StartsWith(TranslateHelper.Translate("Subfolder") + " : "))
            {
                int subfolderspos = (TranslateHelper.Translate("Subfolder") + " : ").Length;
                string subfolder = cmbOutputDirText.ToString().Substring(subfolderspos);

                dirpath = System.IO.Path.GetDirectoryName(filepath) + "\\" + subfolder;
            }
            else if (cmbOutputDirText.Trim() == TranslateHelper.Translate("Overwrite Document"))
            {
                dirpath = System.IO.Path.GetDirectoryName(filepath);

                if (
                    filepath.ToLower().EndsWith(".doc")
                  && !filepath.ToLower().EndsWith(".docx")
                  && !filepath.ToLower().EndsWith(".rtf")
                   )
                {
                    outfilepath=filepath;

                    return outfilepath;
                }
            }
            else
            {
                dirpath = cmbOutputDirText;

                if (rootfolder != string.Empty && Properties.Settings.Default.KeepParentFolderPath)
                {
                    string dep = System.IO.Path.GetDirectoryName(filepath).Substring(rootfolder.Length);

                    dirpath = dirpath + dep;                    
                }                    

            }

            if (!System.IO.Directory.Exists(dirpath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(dirpath);
                }
                catch
                {
                    return null;
                }
            }

            string outfp=
            System.IO.Path.Combine(
                dirpath,
                System.IO.Path.GetFileNameWithoutExtension(filepath) + "_remdupl" + System.IO.Path.GetExtension(filepath)
                );

            if (System.IO.Path.GetExtension(filepath).ToLower() == ".doc"
                || System.IO.Path.GetExtension(filepath).ToLower() == ".docx"
                || System.IO.Path.GetExtension(filepath).ToLower() == ".rtf"
                )
            {
                outfp =
            System.IO.Path.Combine(
                dirpath,
                System.IO.Path.GetFileNameWithoutExtension(filepath) + "_remdupl" + System.IO.Path.GetExtension(filepath)+".txt");
                
            }

            return outfp;
        }

        

        private void SetupOutputFolders()
        {
            if (cmbOutputDir.Items.Count > 0) return;

            cmbOutputDir.Items.Add(TranslateHelper.Translate("Same Folder of Document"));
            cmbOutputDir.Items.Add(TranslateHelper.Translate("Overwrite Document"));
            cmbOutputDir.Items.Add(TranslateHelper.Translate("Subfolder of Document"));
            cmbOutputDir.Items.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString());
            cmbOutputDir.Items.Add("---------------------------------------------------------------------------------------");

            OutputFolderHelper.LoadOutputFolders();
            cmbOutputDir.SelectedIndex = Properties.Settings.Default.OutputFolderIndex;

            if (cmbOutputDir.SelectedIndex == 1)
            {
                chkKeepBackup.Visible = true;
            }

            chkKeepBackup.Checked = Properties.Settings.Default.KeepBackup;

        }

        private void chkKeepBackup_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.KeepBackup = chkKeepBackup.Checked;
        }

        private void cmbOutputDir_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkKeepBackup.Visible = false;

            if (cmbOutputDir.SelectedIndex == 4)
            {
                Module.ShowMessage("Please specify another option as the Output Folder !");
                cmbOutputDir.SelectedIndex = Properties.Settings.Default.OutputFolderIndex;
            }
            else if (cmbOutputDir.SelectedIndex == 2)
            {
                frmOutputSubFolder fob = new frmOutputSubFolder();

                if (fob.ShowDialog() == DialogResult.OK)
                {
                    OutputFolderHelper.SaveOutputFolder(TranslateHelper.Translate("Subfolder") + " : " + fob.txtSubfolder.Text);
                }
                else
                {
                    return;
                }
            }
            else if (cmbOutputDir.SelectedIndex == 1)
            {
                chkKeepBackup.Visible = true;
            }

        }

        public List<BackgroundWorker> lstbw = new List<BackgroundWorker>();

        public string ExecuteCommandLine()
        {
            if (cmbOutputDirText == string.Empty)
            {
                cmbOutputDirText = cmbOutputDir.Text;
            }

            OperationStopped = false;

            Err = "";

            for (int kk = 0; kk < dt.Rows.Count; kk++)
            {
                try
                {

                    string filepath = dt.Rows[kk]["fullfilepath"].ToString();

                    string txt = "";

                    if (!System.IO.File.Exists(filepath))
                    {
                        continue;
                    }
                    else if (System.IO.Path.GetExtension(filepath).ToLower() == ".doc"
                        || System.IO.Path.GetExtension(filepath).ToLower() == ".docx"
                        )
                    {
                        txt = DocumentReaderHelper.ReadWordDocument(filepath);
                    }
                    else if (System.IO.Path.GetExtension(filepath).ToLower() == ".rtf")
                    {
                        txt = DocumentReaderHelper.ReadRTFDocument(filepath);
                    }
                    else
                    {
                        txt = System.IO.File.ReadAllText(filepath);
                    }

                    RemoveDuplicateLinesHelper remdupl = new RemoveDuplicateLinesHelper();

                    if (Properties.Settings.Default.FindWords)
                    {
                        string res = remdupl.RemoveDuplicateWords(txt, Properties.Settings.Default.SortMode);

                        System.IO.File.WriteAllText(GetOutputFilepath(filepath, dt.Rows[kk]["rootfolder"].ToString()), res);
                    }
                    else
                    {
                        string res = remdupl.RemoveDuplicateLines(txt, Properties.Settings.Default.SortMode, true);

                        System.IO.File.WriteAllText(GetOutputFilepath(filepath, dt.Rows[kk]["rootfolder"].ToString()), res);
                    }

                }
                catch (Exception ex)
                {
                    Err += ex.Message + "\n";
                }

            }

            return Err;
        }

        private void tsbReplaceDuplicates_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.CaseSensitiveStringComparison = caseSensitiveStringComparisonToolStripMenuItem.Checked;

            if (donotSortToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.SortMode = 0;
            }
            else if (sortAscendingToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.SortMode = 1;
            }
            else if (sortDescendingToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.SortMode = 2;
            }

            Properties.Settings.Default.FindWords = findWordsNotLinesToolStripMenuItem.Checked;

            cmbOutputDirText = cmbOutputDir.Text;

            Properties.Settings.Default.OutputFolderIndex = cmbOutputDir.SelectedIndex;

            OperationStopped = false;

            Err = "";

            frmProgress f = new frmProgress();
            f.progressBar1.Value = 0;
            f.progressBar1.Maximum = dt.Rows.Count;
            f.Show(this);

            int bwmax = Environment.ProcessorCount;

            for (int k = 0; k < bwmax; k++)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += bw_DoWork;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += bw_ProgressChanged;

                lstbw.Add(bw);
            }

            int kk=0;

            bool[] compl = new bool[dt.Rows.Count];

            for (int k = 0; k < compl.Length; k++)
            {
                compl[k] = false;
            }

            bool busyWord = false;

            while (true)
            {
                if (OperationStopped) break;

                bool found = false;

                bool found2 = false;

                for (int m = 0; m < compl.Length; m++)
                {
                    if (!compl[m])
                    {
                        found = true;

                        if (
                            !(DocumentReaderHelper.BusyWord &&
                            (dt.Rows[m]["fullfilepath"].ToString().ToLower().EndsWith(".doc")
                        || dt.Rows[m]["fullfilepath"].ToString().ToLower().EndsWith(".docx")
                            ))
                            )
                        {
                            kk = m;

                            found2 = true;

                            break;
                        }
                    }
                }

                //if (kk >= dt.Rows.Count) break;                

                if (found && !found2) continue;

                if (!found) break;

                int k = 0;

                while (true)
                {
                    if (!lstbw[k].IsBusy)
                    {
                        compl[kk] = true;

                        List<string> lst1 = new List<string>();
                        lst1.Add(dt.Rows[kk]["fullfilepath"].ToString());
                        lst1.Add(dt.Rows[kk]["rootfolder"].ToString());
 
                        lstbw[k].RunWorkerAsync(lst1);                        

                        //kk++;
                        
                        break;
                    }

                    Application.DoEvents();

                    k++;

                    if (k == lstbw.Count) k = 0;
                }

                Application.DoEvents();
            }

            frmProgress.Instance.Close();

            if (OperationStopped)
            {
                Module.ShowMessage("Operation stopped !");
                return;
            }
            else if (Err != string.Empty)
            {
                frmError fe = new frmError(TranslateHelper.Translate("Operation completed with Errors"), Err);
                fe.Show(this);
            }
            else
            {
                Module.ShowMessage("Operation completed successfully !");
                return;
            }            
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (frmProgress.Instance != null)
            {
                if (frmProgress.Instance.progressBar1.Value < frmProgress.Instance.progressBar1.Maximum)
                {
                    frmProgress.Instance.progressBar1.Value = frmProgress.Instance.progressBar1.Value + 1;
                }
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<string> lst = (List<string>)e.Argument;

                string filepath = lst[0];

                string rootfolder = lst[1];

                string txt = "";

                if (!System.IO.File.Exists(filepath))
                {
                    BackgroundWorker bw = (BackgroundWorker)sender;

                    bw.ReportProgress(0);

                    return;
                }
                else if (System.IO.Path.GetExtension(filepath).ToLower() == ".doc"
                    || System.IO.Path.GetExtension(filepath).ToLower() == ".docx"
                    )
                {
                    txt = DocumentReaderHelper.ReadWordDocument(filepath);
                }
                else if (System.IO.Path.GetExtension(filepath).ToLower() == ".rtf")
                {
                    txt = DocumentReaderHelper.ReadRTFDocument(filepath);
                }
                else
                {
                    txt = System.IO.File.ReadAllText(filepath);
                }

                RemoveDuplicateLinesHelper remdupl = new RemoveDuplicateLinesHelper();

                if (Properties.Settings.Default.FindWords)
                {
                    string res = remdupl.RemoveDuplicateWords(txt, Properties.Settings.Default.SortMode);

                    System.IO.File.WriteAllText(GetOutputFilepath(filepath,rootfolder), res);
                }
                else
                {
                    string res = remdupl.RemoveDuplicateLines(txt, Properties.Settings.Default.SortMode, true);

                    System.IO.File.WriteAllText(GetOutputFilepath(filepath,rootfolder), res);
                }

                BackgroundWorker bw0 = (BackgroundWorker)sender;

                bw0.ReportProgress(0);

            }
            catch (Exception ex)
            {
                frmBatch.Instance.Err += ex.Message + "\r\n";
            }
        }

        private void donotSortToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
