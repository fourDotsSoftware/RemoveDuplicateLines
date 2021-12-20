using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace RemoveDuplicateLines
{
    public partial class frmMain : CustomForm
    {
        public static frmMain Instance = null;

        public RemoveDuplicateLinesHelper RemoveDuplicateLinesHelper = new RemoveDuplicateLinesHelper();

        public frmMain()
        {
            InitializeComponent();

            bwAction.DoWork += bwAction_DoWork;
            bwAction.RunWorkerCompleted += bwAction_RunWorkerCompleted;
            bwAction.WorkerReportsProgress=true;
            bwAction.ProgressChanged += bwAction_ProgressChanged;

            Instance = this;
        }

        public void bwAction_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                int max = (int)e.UserState;

                pgbar.Maximum = max;
            }
            else
            {
                int val = (int)e.UserState;

                if (val <= pgbar.Maximum)
                {
                    pgbar.Value = val;
                }
            }
        }             

        private void frmMain_Resize(object sender, EventArgs e)
        {
            int width0 = this.Width - 22;

            int width = this.Width / 2 - 7;
            txtSource.Width = width;
            txtSource.Left = 0;
            txtSource.Top = 187;
            txtSource.Height = this.Height - txtSource.Top - toolStrip1.Height - 10;

            txtResult.Width = width;
            txtResult.Left = txtSource.Right + 7;
            txtResult.Top = 187;

            txtResult.Height = this.Height - txtResult.Top - toolStrip1.Height - 10;
            
            lblSource.Left = width / 2 - lblSource.Width / 2;
            lblSource.Top = 105;

            lblResult.Left = txtResult.Left + width / 2 - lblResult.Width / 2;
            lblResult.Top = 105;            

            btnCopySource.Top = 89;
            btnClearSource.Top = 124;

            btnCopyResult.Top = 89;
            btnClearResult.Top = 124;

            btnCopySource.Left = txtSource.Right - btnCopyResult.Width - 3;
            btnClearSource.Left = txtSource.Right - btnCopyResult.Width - 3;

            btnCopyResult.Left = this.Width - btnCopyResult.Width - 25;
            btnClearResult.Left = this.Width - btnCopyResult.Width - 25;

            btnNextSource.Left = btnClearSource.Right - btnNextSource.Width;
            btnNextSource.Top = btnClearSource.Bottom + 3;

            btnPrevSource.Left = btnNextSource.Left - btnPrevSource.Width - 3;
            btnPrevSource.Top = btnClearSource.Bottom + 3;

            txtFindSource.Left = btnPrevSource.Left - txtFindSource.Width - 3;
            txtFindSource.Top = btnClearSource.Bottom + 3;

            //====

            btnNextResult.Left = btnClearResult.Right - btnNextResult.Width;
            btnNextResult.Top = btnClearResult.Bottom + 3;

            btnPrevResult.Left = btnNextResult.Left - btnPrevResult.Width - 3;
            btnPrevResult.Top = btnClearResult.Bottom + 3;

            txtFindResult.Left = btnPrevResult.Left - txtFindResult.Width - 3;
            txtFindSource.Top = btnClearResult.Bottom + 3;

            //statusStrip1.Dock = DockStyle.Bottom;

            //statusStrip1.AutoSize = false;
            statusStrip1.Width = width0;
            statusStrip1.Height = 22;

            statusStrip1.Top = this.ClientRectangle.Height - statusStrip1.Height;
            statusStrip1.Left = 0;

            statusStrip1.Width = this.ClientRectangle.Width;
            statusStrip1.Height = 22;

            statusStrip1.BringToFront();
            pgbar.AutoSize = false;
            pgbar.Visible = true;
            pgbar.Width = width0;
            pgbar.Height = 22;
            
            
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
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

            SetupOnLoad();

            if (Properties.Settings.Default.CheckWeek)
            {
                UpdateHelper.InitializeCheckVersionWeek();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            frmMain_Resize(null, null);
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            
        }

        private void btnCopySource_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(txtSource.Text);
        }

        private void btnCopyResult_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(txtResult.Text);
        }

        private void btnClearSource_Click(object sender, EventArgs e)
        {
            txtSource.Text = "";
        }

        private void btnClearResult_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
        }

        public System.ComponentModel.BackgroundWorker bwAction = new System.ComponentModel.BackgroundWorker();

        private string SourceString = "";

        private void tsbRemoveDuplicates_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                foreach (Control co in this.Controls)
                {
                    co.Cursor = Cursors.WaitCursor;
                }

                txtResult.Text = "";

                //pgbar.Visible = true;
                //pgbar.Width = statusStrip1.Width;
                pgbar.Value = 0;

                SourceString = txtSource.Text;

                bwAction.RunWorkerAsync();                

                while (bwAction.IsBusy)
                {
                    Application.DoEvents();
                }
            }
            finally
            {
                this.Cursor = null;

                foreach (Control co in this.Controls)
                {
                    co.Cursor = null;
                }

                //pgbar.Visible = false;
            }
            
        }

        void bwAction_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Properties.Settings.Default.FindWords)
            {
                string res = RemoveDuplicateLinesHelper.RemoveDuplicateWords(SourceString, Properties.Settings.Default.SortMode);

                e.Result = res;
            }
            else
            {
                string res = RemoveDuplicateLinesHelper.RemoveDuplicateLines(SourceString, Properties.Settings.Default.SortMode);

                e.Result = res;
            }
        }

        void bwAction_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtResult.Text = e.Result.ToString();
        }

        private void caseSensitiveStringComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.CaseSensitiveStringComparison = caseSensitiveStringComparisonToolStripMenuItem.Checked;
        }

        private void donotSortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (donotSortToolStripMenuItem.Checked)
            {
                donotSortToolStripMenuItem.Checked = true;
                sortAscendingToolStripMenuItem.Checked = false;
                sortDescendingToolStripMenuItem.Checked = false;
                Properties.Settings.Default.SortMode = 0;
            }
        }

        private void sortAscendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sortAscendingToolStripMenuItem.Checked)
            {
                donotSortToolStripMenuItem.Checked = false;
                sortAscendingToolStripMenuItem.Checked = true;
                sortDescendingToolStripMenuItem.Checked = false;
                Properties.Settings.Default.SortMode = 1;
            }
        }

        private void sortDescendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sortDescendingToolStripMenuItem.Checked)
            {
                donotSortToolStripMenuItem.Checked = false;
                sortAscendingToolStripMenuItem.Checked = false;
                sortDescendingToolStripMenuItem.Checked = true;
                Properties.Settings.Default.SortMode = 2;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.CheckWeek = checkForUpdatesEachWeekToolStripMenuItem.Checked;

            Properties.Settings.Default.Save();
        }

        private void tsbOpenDocument_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();

            opf.Filter = Module.OpenFilesFilter;

            if (Properties.Settings.Default.LastDocument != string.Empty && System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(Properties.Settings.Default.LastDocument)))
            {
                opf.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.LastDocument);
            }

            if (opf.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.LastDocument = opf.FileName;

                string filepath = opf.FileName;

                string txt = "";

                if (System.IO.Path.GetExtension(filepath).ToLower() == ".doc"
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

                txtSource.Text = txt;
            }
        }

        private void tiSelectAllSource_Click(object sender, EventArgs e)
        {
            txtSource.SelectAll();
        }

        private void tiSelectAllResult_Click(object sender, EventArgs e)
        {
            txtResult.SelectAll();
        }

        private void btnNextSource_Click(object sender, EventArgs e)
        {            
            if (txtFindSource.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("Please enter tex to find !");
                return;
            }

            txtSource.Focus();

            if (txtSource.SelectionStart < txtSource.TextLength - 1)
            {
                int stpos = txtSource.SelectionStart + 1;

                if (txtSource.SelectionLength == 0 && txtSource.SelectionStart == 0)
                {
                    stpos = 0;
                }

                int spos = txtSource.Text.IndexOf(txtFindSource.Text, stpos);

                if (spos >= 0)
                {
                    txtSource.SelectionStart = spos;
                    txtSource.SelectionLength = txtFindSource.TextLength;
                    
                }
            }

            //txtFindSource.Focus();
        }

        private void btnPrevSource_Click(object sender, EventArgs e)
        {
            if (txtFindSource.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("Please enter tex to find !");
                return;
            }

            txtSource.Focus();

            int stpos = txtSource.SelectionStart - 1;

            if (txtSource.SelectionLength == 0 && txtSource.SelectionStart == txtSource.TextLength-1)
            {
                stpos = txtSource.SelectionStart;
            }

            int spos = txtSource.Text.IndexOf(txtFindSource.Text, stpos);

            if (txtSource.SelectionStart >0)
            {
                if (spos >= 0)
                {
                    txtSource.SelectionStart = spos;
                    txtSource.SelectionLength = txtFindSource.TextLength;
                    
                }
            }

            //txtFindSource.Focus();
        }

        private void txtFindSource_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnNextSource_Click(null, null);
            }
        }

        private void btnNextResult_Click(object sender, EventArgs e)
        {
            if (txtFindResult.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("Please enter tex to find !");
                return;
            }

            txtResult.Focus();

            if (txtResult.SelectionStart < txtResult.TextLength - 1)
            {
                int stpos = txtResult.SelectionStart + 1;

                if (txtResult.SelectionLength == 0 && txtResult.SelectionStart == 0)
                {
                    stpos = 0;
                }

                int spos = txtResult.Text.IndexOf(txtFindResult.Text, stpos);

                if (spos >= 0)
                {
                    txtResult.SelectionStart = spos;
                    txtResult.SelectionLength = txtFindResult.TextLength;
                    
                }
            }

            //txtFindResult.Focus();
        }

        private void btnPrevResult_Click(object sender, EventArgs e)
        {
            if (txtFindResult.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("Please enter tex to find !");
                return;
            }            

            int stpos = txtResult.SelectionStart - 1;

            if (txtResult.SelectionLength == 0 && txtResult.SelectionStart == txtResult.TextLength - 1)
            {
                stpos = txtResult.SelectionStart;
            }

            int spos = txtResult.Text.LastIndexOf(txtFindResult.Text, stpos);

            txtResult.Focus();

            if (txtResult.SelectionStart > 0)
            {
                if (spos >= 0)
                {
                    txtResult.SelectionStart = spos;
                    txtResult.SelectionLength = txtFindResult.TextLength;
                    
                }
            }

            //txtFindResult.Focus();
        }

        private void txtFindResult_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnNextResult_Click(null, null);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtSource.Text = "";
        }

        private void findWordsNotLinesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FindWords = findWordsNotLinesToolStripMenuItem.Checked;
        }

        private void findWordsNotLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void clearResultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
        }

        #region Help

        private void helpGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(Application.StartupPath + "\\Video Cutter Joiner Expert - User's Manual.chm");
            System.Diagnostics.Process.Start(Module.HelpURL);
        }

        private void pleaseDonateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4dots-software.com/donate.php");
        }

        private void dotsSoftwarePRODUCTCATALOGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4dots-software.com/downloads/4dots-Software-PRODUCT-CATALOG.pdf");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout f = new frmAbout();
            f.ShowDialog();
        }

        private void tiHelpFeedback_Click(object sender, EventArgs e)
        {
            /*
            frmUninstallQuestionnaire f = new frmUninstallQuestionnaire(false);
            f.ShowDialog();
            */

            System.Diagnostics.Process.Start("https://www.4dots-software.com/support/bugfeature.php?app=" + System.Web.HttpUtility.UrlEncode(Module.ShortApplicationTitle));
        }

        private void followUsOnTwitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.twitter.com/4dotsSoftware");
        }

        private void visit4dotsSoftwareWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4dots-software.com");
        }

        private void checkForNewVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateHelper.CheckVersion(false);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Exit();
            }
            catch { }
        }

        #endregion

        #region Localization

        private void AddLanguageMenuItems()
        {
            for (int k = 0; k < frmLanguage.LangCodes.Count; k++)
            {
                ToolStripMenuItem ti = new ToolStripMenuItem();
                ti.Text = frmLanguage.LangDesc[k];
                ti.Tag = frmLanguage.LangCodes[k];
                ti.Image = frmLanguage.LangImg[k];

                if (Properties.Settings.Default.Language == frmLanguage.LangCodes[k])
                {
                    ti.Checked = true;
                }

                ti.Click += new EventHandler(tiLang_Click);

                if (k < 25)
                {
                    languages1ToolStripMenuItem.DropDownItems.Add(ti);
                }
                else
                {
                    languages2ToolStripMenuItem.DropDownItems.Add(ti);
                }

                //languageToolStripMenuItem.DropDownItems.Add(ti);
            }
        }

        void tiLang_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem ti = (ToolStripMenuItem)sender;
            string langcode = ti.Tag.ToString();
            ChangeLanguage(langcode);

            //for (int k = 0; k < languageToolStripMenuItem.DropDownItems.Count; k++)
            for (int k = 0; k < languages1ToolStripMenuItem.DropDownItems.Count; k++)
            {
                ToolStripMenuItem til = (ToolStripMenuItem)languages1ToolStripMenuItem.DropDownItems[k];
                if (til == ti)
                {
                    til.Checked = true;
                }
                else
                {
                    til.Checked = false;
                }
            }

            for (int k = 0; k < languages2ToolStripMenuItem.DropDownItems.Count; k++)
            {
                ToolStripMenuItem til = (ToolStripMenuItem)languages2ToolStripMenuItem.DropDownItems[k];
                if (til == ti)
                {
                    til.Checked = true;
                }
                else
                {
                    til.Checked = false;
                }
            }
        }

        private bool InChangeLanguage = false;

        private void ChangeLanguage(string language_code)
        {
            try
            {
                InChangeLanguage = true;

                Properties.Settings.Default.Language = language_code;
                frmLanguage.SetLanguage();

                bool maximized = (this.WindowState == FormWindowState.Maximized);
                this.WindowState = FormWindowState.Normal;

                /*
                RegistryKey key = Registry.CurrentUser;
                RegistryKey key2 = Registry.CurrentUser;

                try
                {
                    key = key.OpenSubKey("Software\\4dots Software", true);

                    if (key == null)
                    {
                        key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\4dots Software");
                    }

                    key2 = key.OpenSubKey(frmLanguage.RegKeyName, true);

                    if (key2 == null)
                    {
                        key2 = key.CreateSubKey(frmLanguage.RegKeyName);
                    }

                    key = key2;

                    //key.SetValue("Language", language_code);
                    key.SetValue("Menu Item Caption", TranslateHelper.Translate("Change PDF Properties"));
                }
                catch (Exception ex)
                {
                    Module.ShowError(ex);
                    return;
                }
                finally
                {
                    key.Close();
                    key2.Close();
                }
                */
                //1SaveSizeLocation();

                //3SavePositionSize();

                this.Controls.Clear();

                InitializeComponent();

                SetupOnLoad();

                if (maximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                }

                this.ResumeLayout(true);
            }
            finally
            {
                InChangeLanguage = false;
            }
        }

        #endregion

        private void SetupOnLoad()
        {            
            //3this.Icon = Properties.Resources.pdf_compress_48;

            this.Text = Module.ApplicationTitle;
            //this.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            //this.Left = 0;
            AddLanguageMenuItems();

            //3DownloadSuggestionsHelper ds = new DownloadSuggestionsHelper();
            //3ds.SetupDownloadMenuItems(downloadToolStripMenuItem);

            AdjustSizeLocation();

            //3SetupOutputFolders();

            //3keepFolderStructureToolStripMenuItem.Checked = Properties.Settings.Default.KeepFolderStructure;

            /*
            //3
            buyToolStripMenuItem.Visible = frmPurchase.RenMove;

            if (Properties.Settings.Default.Price != string.Empty && !buyApplicationToolStripMenuItem.Text.EndsWith(Properties.Settings.Default.Price))
            {
                buyApplicationToolStripMenuItem.Text = buyApplicationToolStripMenuItem.Text + " " + Properties.Settings.Default.Price;
            }
            */

            /*
             buyToolStripMenuItem.Visible = frmPurchase.RenMove; 
             
            if (Properties.Settings.Default.Price != string.Empty && !buyApplicationToolStripMenuItem.Text.EndsWith(Properties.Settings.Default.Price))
            {
                buyApplicationToolStripMenuItem.Text = buyApplicationToolStripMenuItem.Text + " " + Properties.Settings.Default.Price;
            }
            */

            ResizeControls();

            checkForUpdatesEachWeekToolStripMenuItem.Checked = Properties.Settings.Default.CheckWeek;
        }

        private void AdjustSizeLocation()
        {
            if (Properties.Settings.Default.Maximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {

                if (Properties.Settings.Default.Width == -1)
                {
                    this.CenterToScreen();
                    return;
                }
                else
                {
                    this.Width = Properties.Settings.Default.Width;
                }
                if (Properties.Settings.Default.Height != -1)
                {
                    this.Height = Properties.Settings.Default.Height;
                }

                if (Properties.Settings.Default.Left != -1)
                {
                    this.Left = Properties.Settings.Default.Left;
                }

                if (Properties.Settings.Default.Top != -1)
                {
                    this.Top = Properties.Settings.Default.Top;
                }

                if (this.Width < 300)
                {
                    this.Width = 300;
                }

                if (this.Height < 300)
                {
                    this.Height = 300;
                }

                if (this.Left < 0)
                {
                    this.Left = 0;
                }

                if (this.Top < 0)
                {
                    this.Top = 0;
                }
            }

        }

        private void SaveSizeLocation()
        {
            Properties.Settings.Default.Maximized = (this.WindowState == FormWindowState.Maximized);
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();

        }

        private void batchRemoveDuplicatesForManyFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBatch f = new frmBatch();
            f.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }        
        private void buyApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Module.BuyURL);
        }

        private void enterLicenseKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void tsbSaveResult_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All Files (*.*)|*.*";

            if (System.IO.File.Exists(Properties.Settings.Default.LastDocument))
            {
                if (Properties.Settings.Default.LastDocument.ToLower().EndsWith(".doc")
                    || Properties.Settings.Default.LastDocument.ToLower().EndsWith(".docx")
                    || Properties.Settings.Default.LastDocument.ToLower().EndsWith(".rtf")
                    )
                {
                    sfd.FileName = Properties.Settings.Default.LastDocument + ".txt";
                }
                else
                {
                    sfd.FileName = Properties.Settings.Default.LastDocument;
                }
            }

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(sfd.FileName, txtResult.Text);
            }

        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] filez = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    if (System.IO.File.Exists(filez[0]))
                    {
                        Properties.Settings.Default.LastDocument = filez[0];

                        string filepath = filez[0];

                        string txt = "";

                        if (System.IO.Path.GetExtension(filepath).ToLower() == ".doc"
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

                        txtSource.Text = txt;
                    }
                }
                finally
                {
                    this.Cursor = null;
                }
            }
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
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

        private void frmMain_DragOver(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void commandLineArgumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMessage fm = new frmMessage(true);
            fm.ShowDialog(this);
        }
    }
}
