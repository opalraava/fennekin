using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Common;

namespace WebDiver
{
    public partial class MainWindow : Form
    {
        Data data;
        public static ApplicationData applicationData;
        public static MainWindow mainWindow;
        
        // concurrent child windows
        XSLTExportWindow frmXSLT;
        XSLTImportWindow xsltImportWindow;
 
        public MainWindow()
        {
            InitializeComponent();

            data = new Data();
            applicationData = new ApplicationData();
            engineList = new List<Data.Engine>();
            mainWindow = this;
            
            // init concurrent child windows (but do not show)
            frmXSLT = new XSLTExportWindow();
            xsltImportWindow = new XSLTImportWindow();
            Reset();
        }

        private bool isChanged = false;
        static public string fileName = "";

        static public void Notice(string msg)
        {
            MessageBox.Show(msg);
        }
        
        public void Reset()
        {
            theTreeView.Nodes.Clear();
            theComboBox.Items.Clear();
            currentNode = null;
            BrowseNew();
        }

        // if false, then cancel was pressed.
        private bool saveChanges()
        {
            if (isChanged)
            {
                DialogResult result = MessageBox.Show(
                    "File changed. Do you want to save your changes?",
                    "Web Diver",
                    MessageBoxButtons.YesNoCancel);
		
                if (result == DialogResult.Yes)
                {
                    onSaveDocument();
                    if (isChanged) return false;
                }
                else if (result == DialogResult.No)
                {
                    return true;
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }
        
        private void enableReload(bool value)
        {
            mnuReload.Enabled = value;
        }

        private void setTitleBar()
        {
            if (fileName == "")
                Text = "Web Diver";
            else
                Text = "Web Diver - " + fileName;
        }

        private void FillGUIData()
        { 
            // clear our current control contents
            Reset();

            // fill tree
            foreach (Data.TermTree tTree in data.termTrees)
            {
                FillTreeView(theTreeView, tTree.tree);
            }
        }

        //
        // *** START CODE FROM NodeTree example 
        //
        // This code is only used in the above FillGUIData() function
        //

        private class TreeViewUpdate : IDisposable
        {
            private TreeView TreeView = null;

            public TreeViewUpdate(TreeView TreeView)
            {
                this.TreeView = TreeView;
                TreeView.BeginUpdate();
            }

            public void Dispose()
            {
                TreeView.EndUpdate();
            }        
        }

        private void FillTreeView(TreeView TreeView, ITree tree)
        {
            using (new TreeViewUpdate(TreeView))
            {
                if (tree == null) return;
                foreach (INode child in tree.Root.DirectChildren)
                {
                    AddNodes(TreeView.Nodes, tree.Root);
                }
            }
        }

        private void AddNodes(TreeNodeCollection nodes, INode parent)
        {
            foreach (INode child in parent.DirectChildren)
            {
                Data.Term term = (Data.Term)child.Data;
                TreeNode node = nodes.Add(term.name);
                node.Tag = child;
                AddNodes(node.Nodes, child);
            }
            return;
        }

        //
        // *** RESUME ORIGINAL CODE
        //

        private void onNewDocument()
        {
            if (!saveChanges()) return;

            NewFileWizard frm = new NewFileWizard();
            frm.ShowDialog();
        }
        
        private void onOpenDocument()
        {
            if (!saveChanges()) return;

            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Open WebDiver XML";
                dlg.Filter = "Webdiver files (*.xml)|*.xml|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    DoOpenFile(dlg.FileName);
                }
                dlg.Dispose();
            }
        }

        public void DoOpenFile(string filename)
        {
            fileName = filename;

            Data new_data = new Data();

            if (new_data.ReadFromFile(fileName) == true)
            {
                data = new_data;
                FillGUIData();
                setTitleBar();
                enableReload(true);
                AddRecentFile();
            }
            else
            {
                MessageBox.Show("Unable to load file.");
            }
        }
        
        private void onReloadDocument()
        {
            if (!saveChanges()) return;
            DoOpenFile(fileName);
        }
        
        private void onSaveDocument()
        {
            if (fileName == "")
                onSaveAsDocument();
            else
            {
                data.WriteToFile(fileName);

                isChanged = false;
                // Update title bar to include new name
                setTitleBar();
            }
        }
        
        private void onSaveAsDocument()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save WebDiver XML";
            dlg.DefaultExt = "xml";
            dlg.Filter = "Webdiver files (*.xml)|*.xml|"
            + "All files|*.*";
            dlg.InitialDirectory = "";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                fileName = dlg.FileName;
                onSaveDocument();
                AddRecentFile();
            }

            dlg.Dispose();

        }
        
        private void onViewBack()
        {
            webBrowser.GoBack();
        }
        
        private void onViewForward()
        {
            webBrowser.GoForward();
        }
        
        private void onViewStop()
        {
            webBrowser.Stop();
        }
        
        private void onViewRefresh()
        {
            webBrowser.Refresh();
        }
        
        private void onGo()
        {
            webBrowser.Navigate(tstbAddress.ToString());
        }

        //
        // Most .NET generated event handlers are trivial wrappers
        //
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        { System.Windows.Forms.Application.Exit(); }

        private void toolStrip_LayoutCompleted(object sender, EventArgs e)
        { tstbAddress.Width = (tsbGo.Bounds.Left - tstbAddress.Bounds.Left) - 1; }

        private void toolStrip_Layout(object sender, LayoutEventArgs e)
        { tstbAddress.Width = 1; }

        private void tsbNew_Click(object sender, EventArgs e)
        { onNewDocument(); }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        { onNewDocument(); }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        { onOpenDocument(); }

        private void tsbOpen_Click(object sender, EventArgs e)
        { onOpenDocument(); }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        { onReloadDocument(); }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        { onSaveDocument(); }

        private void tsbSave_Click(object sender, EventArgs e)
        { onSaveDocument(); }

        private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
        { onSaveAsDocument(); }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        { onViewBack(); }

        private void forwardToolStripMenuItem_Click(object sender, EventArgs e)
        { onViewForward(); }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        { onViewStop(); }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        { onViewRefresh(); }

        private void tsbNavBack_Click(object sender, EventArgs e)
        { onViewBack(); }

        private void tsbNavForward_Click(object sender, EventArgs e)
        { onViewForward(); }

        private void tsbNavStop_Click(object sender, EventArgs e)
        { onViewStop(); }

        private void tsbNavRefresh_Click(object sender, EventArgs e)
        { onViewRefresh(); }

        private void tsbGo_Click(object sender, EventArgs e)
        { onGo(); }

        private void tstbAddress_KeyPress(object sender, KeyPressEventArgs e)
        { if (e.KeyChar == '\r') onGo(); }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        { tstbAddress.Text = webBrowser.Url.ToString(); }

//        private void touchCurrentFileToolStripMenuItem_Click(object sender, EventArgs e)
//        {  }

        private void verifyInternalDataToolStripMenuItem_Click(object sender, EventArgs e) 
        { data.TreeVerify(); }

        //
        // MainWindow_Load() - read file when it's given on cmdline
        //

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // load application data from registry
            applicationData.PersistentLoad();
            
            // Add XML editors to main menu
            MainMenuUpdateEditors();
            // Add Recent Files
            MainMenuUpdateRecentFiles();

            // get filename and load it, if given on the commandline
            if (System.Environment.GetCommandLineArgs().Length == 2)
            {
                // get data filename from commandline
                fileName = System.Environment.GetCommandLineArgs()[1];
                if (fileName[0] == '\"' && fileName[fileName.Length - 1] == '\"')
                {
                    fileName = fileName.Replace("\"", "");
                }
                onReloadDocument();
                setTitleBar();
                enableReload(true);
                AddRecentFile();
            }
        }


        /// <summary>
        /// EditorMenuItem and related
        /// </summary>


        public class EditorMenuItem : ToolStripMenuItem
        {
            string mDescription;
            string mExeName;

            public EditorMenuItem(string description, string exename)
                : base(description)
            {
                mDescription = description;
                mExeName = exename;
            }
            
            public static void menuItem_Click(object sender, System.EventArgs e)
            {
                if (mainWindow.saveChanges() == false)
                    return;

                EditorMenuItem menuItem = sender as EditorMenuItem;
                try
                {
                    System.Diagnostics.Process.Start(menuItem.mExeName, "\"" + MainWindow.fileName + "\"");
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to execute editor.");
                }
            }
        }

        private void MainMenuUpdateEditors()
        {
            // 1) delete previous list, if any (keeping the 2 top items)
            int tmp = editWithToolStripMenuItem.DropDownItems.Count;
            for (int i = 2; i < tmp; i++)
                    editWithToolStripMenuItem.DropDownItems.RemoveAt(2);

            // 2) add the new enginelist
            foreach (ApplicationData.EditorItem item in applicationData.editorList)
            {
                EditorMenuItem menuItem = new EditorMenuItem(item.menuName,item.exeFilename);
                menuItem.Click += new EventHandler(EditorMenuItem.menuItem_Click);
                editWithToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        public class RecentFileMenuItem : ToolStripMenuItem
        {
            string mFilename;
 
            public RecentFileMenuItem(string filename)
                : base(filename)
            {
                mFilename = filename;
            }

            public static void menuItem_Click(object sender, System.EventArgs e)
            {
                if (!mainWindow.saveChanges()) return;

                RecentFileMenuItem menuItem = sender as RecentFileMenuItem;
                try
                {
                    mainWindow.DoOpenFile(menuItem.mFilename);
                }
                catch (Exception)
                {
                    // the recent file has been deleted or moved, so remove
                    // it from the recent file list (Issue #10)
                    applicationData.recentFiles.Remove(menuItem.mFilename);
                    mainWindow.MainMenuUpdateRecentFiles();
                    MessageBox.Show("File not found. It's removed from the recent files.");
                }
            }
        }


        private void MainMenuUpdateRecentFiles()
        {
            // 1) delete previous list
            recentFilesToolStripMenuItem.DropDownItems.Clear();

            // 2) add the new enginelist
            foreach (string item in applicationData.recentFiles)
            {
                RecentFileMenuItem menuItem = new RecentFileMenuItem(item);
                menuItem.Click += new EventHandler(RecentFileMenuItem.menuItem_Click);
                recentFilesToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void AddRecentFile()
        {
            const int constMaxRecentFiles = 10;

            if (fileName != "")
            {
                List<string> tmp = new List<string>();

                tmp.Add(fileName);  // add 'filename' to top of current list
                
                foreach(string item in applicationData.recentFiles)
                {
                    if (item != fileName && item != "")
                        tmp.Add(item);

                    if (tmp.Count >= constMaxRecentFiles)
                        break;
                }

                applicationData.recentFiles.Clear();
                foreach (string s in tmp) applicationData.recentFiles.Add(s);

                MainMenuUpdateRecentFiles();
            }
        }


        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (saveChanges() == false) /* user pressed 'cancel' */
            {
                e.Cancel = true;
            }
        }

        //
        // Tree navigation event handlers.
        //
        
        TreeNode currentNode = null;
        bool passedBeforeExpand = false;

        private void theTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (currentNode == e.Node)
            {
                if (passedBeforeExpand == false)
                    BrowseNew();
                else
                    passedBeforeExpand = false;
            }
        }

        private void theTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            currentNode = e.Node;
            BrowseNew();
        }

        private void theTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            passedBeforeExpand = true;
        }

        // the combobox event handler
        private void theComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            BrowseNew();
        }

        //
        // Common event handler for when a new page must be displayed
        //

        private bool inSelectQuery = false;

        private void BrowseNew()
        {
            if (inSelectQuery) return;

            if (currentNode == null) {
                tstbAddress.Text = "about:blank";
                onGo();
                return;
            }

            // 'select' query, $topic and $encoded_topic values
            string query = SelectQuery();

            string term = currentNode.Text;
            string encoded_term = System.Web.HttpUtility.UrlEncode(term, new UTF8Encoding());
            string arxiv_term = make_arxiv_term(term);

            string result = query;

            // OLD SYNTAX
            result = result.Replace("$encoded_term", encoded_term);
            result = result.Replace("$term", term);
            result = result.Replace("$arxiv", arxiv_term);

            // NEW SYNTAX
            result = result.Replace("$(encoded_term)", encoded_term);
            result = result.Replace("$(term)", term);
            result = result.Replace("$(arxiv)", arxiv_term);

            
            // we want to go to this URL
            string location = result;

            // kick the gui
            tstbAddress.Text = location;
            onGo();
        }

        string make_arxiv_term(string term)
        {

            // http://arxiv.org/find/all/1/abs:+AND+theory+AND+quantum+AND+information+group/0/1/0/all/0/1
            //
            // in <engine/> : "http://arxiv.org/find/all/1/abs:$(arxiv)/0/1/0/all/0/1"
            // so we need to
            // replace: "theory quantum information group"
            // with     "+AND+theory+AND+quantum+AND+information+group"
            // with     "+AND+theory | +AND+quantum | +AND+information | +group"

            string[] seperators = new string[] { " ", "\t" };
            string[] result = term.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            string retval = "";

            for (int i = 0; i < result.Length; i++)
            {
                if (i == result.Length - 1)
                {
                    // last element
                    retval += "+" + result[i];
                }
                else
                {
                    retval += "+AND+" + result[i];
                }
            }

            return retval;
        }

        //
        // Cascading search engine selection
        //

        List<Data.Engine> engineList;

        private string SelectQuery()
        {
            inSelectQuery = true;
            INode currNode = (INode)currentNode.Tag;

            //
            // add engines and sets while walking to the root term
            //

            engineList.Clear();

            do
            {
                Data.Term term = (Data.Term) currNode.Data;
                
                // add them engines
                foreach (Data.Engine engine in term.engines)
                {
                    AddEngine(engine.name,engine.query);
                }

                // add them enginesets, in reverse
                List<string> tmp = term.enginesets;
                tmp.Reverse();
                foreach (string name in tmp)
                {
                    AddEngineSet(name);
                }

                // walk to the parent, if possible
                if (currNode.IsRoot) break;
                currNode = currNode.Parent;
                if (currNode.IsRoot) break;
            } while (true);

            // add root engines
            AddRootEngines();

            //
            // update listbox ctl
            //


            // first, 'remember' current engine
            string tmpEnginename = theComboBox.Text;

            theComboBox.Items.Clear();
            foreach (Data.Engine e in engineList)
            {
                theComboBox.Items.Add(e.name);
            }

            // 'restore' index
            int i=0;
            foreach (string s in theComboBox.Items)
            {
                if (s == tmpEnginename)
                {
                    theComboBox.SelectedIndex = i;
                }
                i++;
            }


            // ok, assume we got the right engine name selected
            if (theComboBox.Text == "")
            {
                inSelectQuery = false;
                return "about:blank";
            }
            // find the engine
            foreach (Data.Engine e in engineList)
            {
                if (e.name == theComboBox.Text)
                {
                    inSelectQuery = false;
                    return e.query;
                }
            }


            inSelectQuery = false;
            return "about:notreached error";
        }

        private void AddEngine(string name, string query)
        {

            // add this engine (if it not already exists)
            foreach (Data.Engine e in engineList)
            {
                if (e.name == name) return;
            }
            engineList.Add(new Data.Engine(name, query));
        }

        private void AddRootEngines()
        {
            foreach (Data.Engine e in data.rootEngines)
                AddEngine(e.name, e.query);
        }

        private void AddEngineSet(string name)
        {
            // add the contents of this engine set using AddEngine() for each engine
            foreach (Data.EngineSet engineSet in data.engineSets)
            {
                if (engineSet.name == name)
                    foreach (Data.Engine engine in engineSet)
                    {
                        AddEngine(engine.name, engine.query);
                    }
            }
        }

//        private void showDocumentStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//        }

//        private void configureEditorsToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//        }

        private void runThisFileInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("\"" + fileName + "\"");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About frm = new About();
            frm.ShowDialog();
        }

        private void documentatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/webdiver/wiki/WebDiver");
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            applicationData.PersistentSave();
        }


        private void theTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
//            MessageBox.Show("theTreeView_AfterLabelEdit");
        }

        private void showStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            data.TreeVerify();
        }

        private void configureEditorsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ConfigureEditorsForm frm = new ConfigureEditorsForm();
            frm.ShowDialog();
            MainMenuUpdateEditors();
        }

        private void touchDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isChanged = true;
        }

        private void xSLProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // run and exit this function (keep window async)
            frmXSLT.ShowDialog();
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array a = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (a != null)
                {
                    // Extract string from first array element
                    // (ignore all files except first if number of files are dropped).
                    string s = a.GetValue(0).ToString();

                    // Call OpenFile asynchronously.
                    // Explorer instance from which file is dropped is not responding
                    // all the time when DragDrop handler is active, so we need to return
                    // immidiately (especially if OpenFile shows MessageBox).

//                    this.BeginInvoke(m_DelegateOpenFile, new Object[] { s });
//
//                    this.Activate();        // in the case Explorer overlaps this form
                }
            }
            catch (Exception)
            {
                // MessageBox.Show("Error in DragDrop function: " + ex.Message);
                // don't show MessageBox here - Explorer is waiting !
            }
        }

        private void theTreeView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UnicodeText", true))
            {
                string s = (string)e.Data.GetData("UnicodeText", true);
                Point point = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(point);

                {
                    INode tmp = (INode)destinationNode.Tag;
                    Data.Term term = new Data.Term();
                    term.name = s;

                    INode child = tmp.AddChild(term);

                    TreeNode newNode = new TreeNode(s);
                    newNode.Tag = child;

                    destinationNode.Nodes.Add(newNode);
                    destinationNode.Expand();

                    isChanged = true;
                }
            }
        }

        private void theTreeView_DragEnter(object sender, DragEventArgs e)
        {

            // a verbatim string
            string s = (string) e.Data.GetData(DataFormats.Text);
            if (s != null)
            {
                // this is a string dragged into the tree
                e.Effect = DragDropEffects.Copy;
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                // a file has been dropped into the tree.
                MessageBox.Show("A file has been dropped into the tree, import it.");
                e.Effect = DragDropEffects.Link;
                return;
            }
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }

        private void xSLImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xsltImportWindow.ShowDialog();
        }

        //
        //
        //

        public void NewFile_Generate(string engineSetName, string termName)
        {
            Data.EngineSet eSet = new Data.EngineSet(engineSetName);

            if (engineSetName == " English - default")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://en.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com/images?q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.com/search?q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("youtube", "http://www.youtube.com/results?search_query=$encoded_term"));
            }
            if (engineSetName == " English - balanced")
            {
                eSet.Add(new Data.Engine("google blogseach", "http://blogsearch.google.com/blogsearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("google books", "http://books.google.com/books?q=$encoded_term&as_brr=1"));
                eSet.Add(new Data.Engine("google code search", "http://www.google.com/codesearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com/images?q=$encoded_term"));
                eSet.Add(new Data.Engine("google news", "http://news.google.com/news?q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.com/search?q=$encoded_term"));
                eSet.Add(new Data.Engine("google video", "http://video.google.com/videosearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("google scholar", "http://scholar.google.com/scholar?q=intitle:$encoded_term"));
                eSet.Add(new Data.Engine("wikipedia", "http://en.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("youtube", "http://www.youtube.com/results?search_query=$encoded_term"));
            }
            else if (engineSetName == " English - extended")
            {
                eSet.Add(new Data.Engine("google search", "http://www.google.com/search?q=$encoded_term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com/images?q=$encoded_term"));
                eSet.Add(new Data.Engine("google video", "http://video.google.com/videosearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("google news", "http://news.google.com/news?q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogseach", "http://blogsearch.google.com/blogsearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("google code search", "http://www.google.com/codesearch?q=$encoded_term"));
                eSet.Add(new Data.Engine("google trends", "http://www.google.com/trends?q=$encoded_term"));
                eSet.Add(new Data.Engine("yahoo search", "http://search.yahoo.com/search?p=$term&fr=yfp-t-501&toggle=1&cop=mss&ei=UTF-8"));
                eSet.Add(new Data.Engine("yahoo images", "http://images.search.yahoo.com/search/images?p=$term&fr=yfp-t-501&toggle=1&ei=UTF-8&fr2=tab-web"));
                eSet.Add(new Data.Engine("yahoo video", "http://video.search.yahoo.com/search/video?p=$term&fr=yfp-t-501&toggle=1&ei=UTF-8&fr2=tab-img"));
                eSet.Add(new Data.Engine("yahoo audio", "http://audio.search.yahoo.com/search/audio?p=$term&fr=yfp-t-501&toggle=1&ei=UTF-8&fr2=tab-vid"));
                eSet.Add(new Data.Engine("yahoo news", "http://news.search.yahoo.com/search/news?p=$term"));
                eSet.Add(new Data.Engine("ask.com", "http://www.ask.com/web?q=$term&qsrc=0&o=312&l=dir"));
                eSet.Add(new Data.Engine("microsoft search", "http://search.live.com/results.aspx?q=$term&mkt=en-us&FORM=LVSP"));
                eSet.Add(new Data.Engine("microsoft images", "http://search.live.com/images/results.aspx?q=$term&FORM=BIRE"));
                eSet.Add(new Data.Engine("microsoft news", "http://search.live.com/news/results.aspx?q=$term&FORM=BNIR"));
                eSet.Add(new Data.Engine("wikipedia", "http://en.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("youtube", "http://www.youtube.com/results?search_query=$encoded_term"));
                eSet.Add(new Data.Engine("amazon", "http://www.amazon.com/s/ref=nb_ss_/002-2408646-9740015?url=search-alias=aps&field-keywords=$term"));
                eSet.Add(new Data.Engine("ebay", "http://search.ebay.com/$term"));
                eSet.Add(new Data.Engine("technorati.com", "http://www.technorati.com/search/$term"));
                eSet.Add(new Data.Engine("del.icio.us", "http://del.icio.us/search/?fr=del_icio_us&p=$term&type=all"));
                eSet.Add(new Data.Engine("arxiv", "http://arxiv.org/find/all/1/abs:$(arxiv)/0/1/0/all/0/1"));
            }
            else if (engineSetName == "Idioma español")
            {
                eSet.Add(new Data.Engine("wikipedia","http://es.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images","http://images.google.es/images?meta=lr%3Dlang_es&hl=es&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search","http://www.google.es/search?meta=lr%3Dlang_es&hl=es&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch","http://blogsearch.google.com/blogsearch?hl=es&q=$encoded_term&lr=lang_es"));
            }
            else if (engineSetName == "Deutsche Sprache")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://de.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.de/images?meta=lr%3Dlang_de&hl=de&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.de/search?meta=lr%3Dlang_de&hl=de&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=de&q=$encoded_term&lr=lang_de"));
            }
            else if (engineSetName == "Français")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://fr.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.fr/images?meta=lr%3Dlang_fr&hl=fr&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.fr/search?meta=lr%3Dlang_fr&hl=fr&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=fr&q=$encoded_term&lr=lang_fr"));
            }
            else if (engineSetName == "Nederlands")
            {
                eSet.Add(new Data.Engine("youtube","http://www.youtube.com/results?search_type=&search_query=$encoded_term&aq=f"));
                eSet.Add(new Data.Engine("wikipedia", "http://nl.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google plaatjes", "http://images.google.nl/images?hl=nl&q=$encoded_term&btnG=Afbeeldingen+zoeken&gbv=2"));
                eSet.Add(new Data.Engine("google zoeken", "http://www.google.nl/search?hl=nl&q=$encoded_term&btnG=Google+zoeken&meta=lr=lang_nl"));
                eSet.Add(new Data.Engine("google in blogs zoeken", "http://blogsearch.google.com/blogsearch?hl=nl&q=$encoded_term&btnG=Zoeken+in+blogs&lr="));
                eSet.Add(new Data.Engine("google nieuws", "http://news.google.nl/news?hl=nl&ned=nl_nl&q=$encoded_term&ned=nl_nl&q=$encoded_term"));
                eSet.Add(new Data.Engine("veilingkijker", "http://www.veilingkijker.nl/index.cgi?zoekopdracht=$term"));
                eSet.Add(new Data.Engine("vandale", "http://www.vandale.nl/opzoeken/woordenboek/?zoekwoord=$term"));
                eSet.Add(new Data.Engine("bol", "http://www.nl.bol.com/is-bin/INTERSHOP.enfinity/eCS/Store/nl/-/EUR/BOL_ParametricSearch-Start;sid=nrXm2bU-sQDm0vLehm7Y7lXpBFaCVqqFK0w=?Ntk=nl_books&Nty=1&Section=BOOK&Ntt=$term"));
            }
            else if (engineSetName == "Lingua Italiana")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://it.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.it/images?meta=lr%3Dlang_it&hl=it&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.it/search?meta=lr%3Dlang_it&hl=it&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=it&q=$encoded_term&lr=lang_it"));
           }
            else if (engineSetName == "Lingua Latina")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://la.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com/images?meta=lr%3Dlang_la&hl=la&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.com/search?meta=lr%3Dlang_la&hl=la&q=$encoded_term"));
            }
            else if (engineSetName == "Ελληνική γλώσσα")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://el.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.gr/images?meta=lr%3Dlang_el&hl=el&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.gr/search?hl=el&meta=lr%3Dlang_el&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=el&q=$encoded_term&lr=lang_el"));
            }
            else if (engineSetName == "Русский язык")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://ru.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.ru/images?meta=lr%3Dlang_ru&hl=ru&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.ru/search?meta=lr%3Dlang_ru&hl=ru&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=ru&q=$encoded_term&lr=lang_ru"));
            }
            else if (engineSetName == "Türkçe")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://tr.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com.tr/images?meta=lr%3Dlang_tr&hl=tr&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.com.tr/search?meta=lr%3Dlang_tr&hl=tr&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=tr&q=$encoded_term&lr=lang_tr"));
            }
            else if (engineSetName == "日本語")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://ja.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.co.jp/images?meta=lr%3Dlang_ja&hl=ja&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.co.jp/search?meta=lr%3Dlang_ja&hl=ja&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=ja&q=$encoded_term&lr=lang_ja"));
            }
            else if (engineSetName == "中文")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://zh.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.cn/images?meta=lr%3Dlang_zh-CN&hl=zh-CN&q=$encoded_term"));
                eSet.Add(new Data.Engine("google search", "http://www.google.cn/search?meta=lr%3Dlang_zh-CN&hl=zh-CN&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=zh-CN&q=$encoded_term&lr=lang_zh-CN"));
            }
            else if (engineSetName == "العربية")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://ar.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com/images?q=$encoded_term&hl=ar"));
                eSet.Add(new Data.Engine("google search", "http://www.google.com/search?hl=ar&lr=lang_ar&q=$encoded_term"));
                eSet.Add(new Data.Engine("google blogsearch", "http://blogsearch.google.com/blogsearch?hl=ar&q=$encoded_term&lr=lang_ar"));
            }
            else if (engineSetName == "हिन्दी")
            {
                eSet.Add(new Data.Engine("wikipedia", "http://hi.wikipedia.org/wiki/$term"));
                eSet.Add(new Data.Engine("google images", "http://images.google.com/images?q=$encoded_term&hl=hi"));
                eSet.Add(new Data.Engine("google search", "http://www.google.com/search?hl=hi&q=$encoded_term"));
            }

            Data new_data = new Data();

            new_data.GenerateNewFileWizard(eSet, termName);
            data = new_data;

            fileName = "";
            isChanged = true;
            FillGUIData();
            setTitleBar();
            enableReload(false);
        }
    }
}