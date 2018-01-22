using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Dullware.Library;
using Dullware.Plotter;
using CarlosAg.ExcelXmlWriter;

[assembly: System.Reflection.AssemblyProduct("AfterALV")]
[assembly: System.Reflection.AssemblyDescription("AfterALV 1.0f")]
[assembly: System.Reflection.AssemblyTitle("AfterALV by Dullware")]
[assembly: System.Reflection.AssemblyCompany("Dullware")]
[assembly: System.Reflection.AssemblyCopyright("(c) 2006 - 2015 Dullware")]
[assembly: System.Reflection.AssemblyVersion("1.0.*")]

public class AfterALV : DullForm
{
    ToolStrip tool;
    ToolStripButton bFileNew, bFileOpen, bFileSave;
    ToolStripButton bProjectAdd, bProjectRemove, bProjectExport;

    ImageList imagelist;
    MenuStrip menu;
    ToolStripMenuItem mFile, mFileNew, mFileOpen, mFileClose, mFileSave, mFileSaveAs, mFilePrint, mFilePrintGraph, mFileExit;
    ToolStripMenuItem mProject, mProjectAdd, mProjectRemove, mProjectExportXML, mProjectExportCSV, mProjectExportInputFile;
    ToolStripMenuItem mGraph, mGraphChangeColor, mGraphPrint;

    TabControl AngleTabControl = new TabControl();

    Timer timer;
    ToolTip tips;

    [STAThread]
    public static void Main(string[] args)
    {
        //args = new string[] { @"W:\src\dls\testdir\1en3 old.aap" };
        //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("",false);
        //System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator=".";
        //System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator=",";
        Application.EnableVisualStyles();
        NewFormRequest += new NewFormEventHandler(AfterALV_NewFormRequest);
        Run(args, new StartupCheck(Verify));
    }

    static bool Verify()
    {	// in the next two line the "phone home" is disabled.
    	//return (System.Net.Dns.GetHostName() == "qhuiskamer" || System.Net.Dns.GetHostName() == "qatv-pcc-03006" || System.Net.Dns.GetHostName() == "kleinepc" || (new StartupScreen()).ShowDialog() == DialogResult.OK)&& (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator=="." || MessageBox.Show("You are currently not using a decimal point, and CONTIN cannot handle this. \r\n\r\nPlease change the setting in your Control Panel (regional settings), and try again.",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Stop) != DialogResult.OK);
    	return (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator=="." || MessageBox.Show("You are currently not using a decimal point, and CONTIN cannot handle this. \r\n\r\nPlease change the setting in your Control Panel (regional settings), and try again.",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Stop) != DialogResult.OK);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    public AfterALV()
    {
        UntitledPrefix = "Project";
        //FileNameFilter = "AfterALV Project Files (*.aap)|*.aap|All files (*.*)|*.*";        
        FileNameFilter = "AfterALV Project Files (*.aap)|*.aap";
        FileNameDefaultExt = "aap";
        AdditionalFileDropExtensions = new string[] { ".dat", ".asc" };

        //Icon = new Icon(Application.StartupPath + @"\AfterALV.ico");
        Icon = new Icon(GetType(), "AfterALV.AfterALV.ico");

        imagelist = new ImageList();
        imagelist.Images.Add("FileNew", new Bitmap(GetType(), "AfterALV.new_document_16.bmp"));
        imagelist.Images.Add("FileOpen", new Bitmap(GetType(), "AfterALV.open_document_16.bmp"));
        imagelist.Images.Add("FileSave", new Bitmap(GetType(), "AfterALV.save_16.bmp"));
        imagelist.Images.Add("ProjectAdd", new Bitmap(GetType(), "AfterALV.MoveToFolder.bmp"));
        imagelist.Images.Add("ProjectRemove", new Bitmap(GetType(), "AfterALV.delete_x_16.bmp"));
        imagelist.Images.Add("ProjectExport", new Bitmap(GetType(), "AfterALV.move_to_folder_16.bmp"));
        imagelist.TransparentColor = Color.Fuchsia;
        imagelist.ColorDepth = ColorDepth.Depth24Bit;

        MakeMenuStrip();
        MakeToolStrip();
        AngleTabControl.Parent = this;
        tool.Parent = this;
        menu.Parent = this;

        new StatusStrip().Parent = this;

        tips = new ToolTip();
        tips.SetToolTip(this, "Main window");

        Width = 880;
        Height = 740;

        timer = new Timer();
        timer.Interval = 500;
        timer.Tick += new EventHandler(timer_Tick);
        timer_Tick(this, EventArgs.Empty);
        timer.Start();

        AngleTabControl.Dock = DockStyle.Fill;
        AddDataFile();
    }

    protected override void OnZombieDocumentChanged(EventArgs eventArgs)
    {
        base.OnZombieDocumentChanged(eventArgs);
        AngleTabControl.Visible = !ZombieDocument;
        if (ZombieDocument) AngleTabControl.TabPages.Clear();
    }

    void timer_Tick(object sender, EventArgs e)
    {
        bFileNew.Enabled = true;
        bFileOpen.Enabled = true;
        bFileSave.Enabled = Changed;
        bProjectAdd.Enabled = !ZombieDocument;

        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null)
        {
            if (atp.ContinRuns)
            {
                bProjectAdd.Enabled = false;
                bProjectRemove.Enabled = false;
                bProjectExport.Enabled = false;
            }
            else
            {
                if (atp.bRunContin != null) atp.bRunContin.Enabled = atp.listviewdatafiles.CheckedItems.Count > 0;
                bProjectRemove.Enabled = atp.listviewdatafiles.Enabled && atp.listviewdatafiles.SelectedItems.Count > 0;
                bProjectExport.Enabled = atp.listviewdatafiles.Items.Count > 0;
            }
        }
        else
        {
            bProjectRemove.Enabled = false;
            bProjectExport.Enabled = false;
        }
    }

    static void AfterALV_NewFormRequest(NewFormEventArgs frea)
    {
        frea.form = new AfterALV();
    }

    void MakeToolStrip()
    {
        tool = new ToolStrip();
        tool.ImageList = imagelist;

        bFileNew = new ToolStripButton();
        bFileNew.ToolTipText = "New project";
        bFileNew.ImageKey = "FileNew";
        bFileNew.Click += FileNew_Click;
        tool.Items.Add(bFileNew);

        bFileOpen = new ToolStripButton();
        bFileOpen.ToolTipText = "Open project";
        bFileOpen.ImageKey = "FileOpen";
        bFileOpen.Click += FileOpen_Click;
        tool.Items.Add(bFileOpen);

        bFileSave = new ToolStripButton();
        bFileSave.ToolTipText = "Save project";
        bFileSave.ImageKey = "FileSave";
        bFileSave.Click += FileSave_Click;
        tool.Items.Add(bFileSave);

        bProjectAdd = new ToolStripButton();
        bProjectAdd.ToolTipText = "Add data files to the project";
        bProjectAdd.ImageKey = "ProjectAdd";
        bProjectAdd.Click += ProjectAdd_Click;
        tool.Items.Add(bProjectAdd);

        bProjectRemove = new ToolStripButton();
        bProjectRemove.ToolTipText = "Remove data files from the project";
        bProjectRemove.ImageKey = "ProjectRemove";
        bProjectRemove.Click += new EventHandler(ProjectRemove_Click);
        tool.Items.Add(bProjectRemove);

        bProjectExport = new ToolStripButton();
        bProjectExport.ToolTipText = "Export data as an XML Spreadsheet file";
        bProjectExport.ImageKey = "ProjectExport";
        bProjectExport.Click += new EventHandler(ProjectExportXML_Click);
        tool.Items.Add(bProjectExport);
    }

    void MakeMenuStrip()
    {
        menu = new MenuStrip();
        menu.ShowItemToolTips = true;

        mFile = new ToolStripMenuItem("&File");
        mFile.DropDown.ImageList = imagelist;
        mFile.DropDownOpening += new EventHandler(mFile_DropDownOpening);
        mFile.DropDownClosed += new EventHandler(mFile_DropDownClosed);

        mFileNew = new ToolStripMenuItem("&New Project", null, FileNew_Click, Keys.Control | Keys.N);
        mFileNew.ImageKey = "FileNew";

        mFileOpen = new ToolStripMenuItem("&Open Project...", null, FileOpen_Click, Keys.Control | Keys.O);
        mFileOpen.ImageKey = "FileOpen";

        mFileClose = new ToolStripMenuItem("&Close Project", null, FileClose_Click, Keys.Control | Keys.W);

        mFileSave = new ToolStripMenuItem("&Save Project", null, FileSave_Click, Keys.Control | Keys.S);

        mFileSaveAs = new ToolStripMenuItem("Save Project &As...", null, FileSaveAs_Click, Keys.Control | Keys.S | Keys.Shift);

        mFilePrint = new ToolStripMenuItem("&Print Project...", null, FilePrint_Click, Keys.Control | Keys.P);

        mFilePrintGraph = new ToolStripMenuItem("&Print graph...", null, FilePrintGraph_Click, Keys.Control | Keys.P | Keys.Shift);

        mFileExit = new ToolStripMenuItem("E&xit", null, FileExit_Click, Keys.Control | Keys.Q);

        mFile.DropDownItems.AddRange(new ToolStripItem[] { mFileNew, mFileOpen, new ToolStripSeparator(), mFileClose, mFileSave, mFileSaveAs, new ToolStripSeparator(), mFilePrint, mFilePrintGraph, new ToolStripSeparator(), mFileExit });

        mProject = new ToolStripMenuItem("&Project");
        mProject.DropDown.ImageList = imagelist;
        mProject.DropDownOpening += new EventHandler(mProject_DropDownOpening);
        mProject.DropDownClosed += new EventHandler(mProject_DropDownClosed);

        mProjectAdd = new ToolStripMenuItem("&Add Data File(s)...", null, ProjectAdd_Click, Keys.Control | Keys.D);
        mProjectAdd.ImageKey = "ProjectAdd";

        mProjectRemove = new ToolStripMenuItem("&Remove Data File(s)...", null, ProjectRemove_Click, Keys.Control | Keys.R);
        mProjectRemove.ImageKey = "ProjectRemove";

        mProjectExportXML = new ToolStripMenuItem("&Export data as XML...", null, ProjectExportXML_Click, Keys.Control | Keys.E);
        mProjectExportXML.ImageKey = "ProjectExport";

        mProjectExportCSV = new ToolStripMenuItem("&Export data as CSV...", null, ProjectExportCSV_Click);

        mProjectExportInputFile = new ToolStripMenuItem("&Export contin input file...", null, ProjectExportInputFile_Click, null);

        mProject.DropDownItems.AddRange(new ToolStripItem[] { mProjectAdd, mProjectRemove, new ToolStripSeparator(), mProjectExportXML, mProjectExportCSV, new ToolStripSeparator(), mProjectExportInputFile });

        mGraph = new ToolStripMenuItem("&Graph");
        mGraph.DropDown.ImageList = imagelist;
        mGraph.DropDownOpening += new EventHandler(mGraph_DropDownOpening);
        mGraph.DropDownClosed += new EventHandler(mGraph_DropDownClosed);

        mGraphChangeColor = new ToolStripMenuItem("&Change Line Color...", null, GraphChangeColor_Click, null);

        mGraphPrint = new ToolStripMenuItem("&Print graph...", null, GraphPrint_Click, null);

        mGraph.DropDownItems.AddRange(new ToolStripItem[] { mGraphChangeColor, new ToolStripSeparator(), mGraphPrint });

        menu.Items.AddRange(new ToolStripMenuItem[] { mFile, mProject, mGraph, WindowMenu, HelpMenu, CloseMenu });
    }

    void mFile_DropDownOpening(object sender, EventArgs e)
    {
        mFileClose.Enabled = !ZombieDocument;
        mFileSave.Enabled = Changed && !ZombieDocument;
        mFileSaveAs.Enabled = (Changed || !Untitled) && !ZombieDocument;
        mFilePrint.Enabled = !ZombieDocument && (AngleTabPage)AngleTabControl.SelectedTab != null;
        mFilePrintGraph.Enabled = false;


        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null)
        {
            mFilePrintGraph.Enabled = atp.plotCountRates.ContainsFocus || atp.plotCorrelations.ContainsFocus || atp.plotFit.ContainsFocus || atp.plotTransform.ContainsFocus;
        }
    }

    void mFile_DropDownClosed(object sender, EventArgs e)
    {
        foreach (ToolStripItem it in mFile.DropDownItems)
        {
            if (it is ToolStripMenuItem) it.Enabled = true;
        }
    }

    static System.Drawing.Printing.PrintDocument printproject = new System.Drawing.Printing.PrintDocument();
    int startingprinttab;

    void FilePrint_Click(object sender, EventArgs e)
    {
        if (ZombieDocument) return;

        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null)
        {
            printproject.PrintPage += printproject_PrintPage;
            printproject.DocumentName = Name;
            PrintDialog pdial = new PrintDialog();
            pdial.Document = printproject;
            if (pdial.ShowDialog() == DialogResult.OK)
            {
                startingprinttab = 0;
                printproject.Print();
                //PrintPreviewDialog ppd = new PrintPreviewDialog();
                //ppd.Document = printproject;
                //ppd.ShowDialog();
                //ppd.Dispose();
            }
            printproject.PrintPage -= printproject_PrintPage;
            pdial.Dispose();
        }
    }

    void FilePrintGraph_Click(object sender, EventArgs e)
    {
        if (ZombieDocument) return;

        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null)
        {
            if (atp.plotCountRates.ContainsFocus) atp.plotCountRates.Print(sender, e);
            else if (atp.plotCorrelations.ContainsFocus) atp.plotCorrelations.Print(sender, e);
            else if (atp.plotFit.ContainsFocus) atp.plotFit.Print(sender, e);
            else if (atp.plotTransform.ContainsFocus) atp.plotTransform.Print(sender, e);
        }
    }

    void mProject_DropDownOpening(object sender, EventArgs e)
    {
        mProjectAdd.Enabled = !ZombieDocument;
        mProjectRemove.Enabled = false;
        mProjectExportXML.Enabled = false;
        mProjectExportCSV.Enabled = false;
        mProjectExportInputFile.Enabled = false;

        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null)
        {
            mProjectAdd.Enabled = !atp.ContinRuns;
            mProjectRemove.Enabled = !atp.ContinRuns && atp.listviewdatafiles.Enabled && atp.listviewdatafiles.SelectedItems.Count > 0;
            mProjectExportXML.Enabled = !atp.ContinRuns && atp.listviewdatafiles.Items.Count > 0;
            mProjectExportCSV.Enabled = !atp.ContinRuns && atp.listviewdatafiles.Items.Count > 0;
            mProjectExportInputFile.Enabled = !atp.ContinRuns && atp.listviewdatafiles.CheckedItems.Count > 0;
        }
    }

    void mProject_DropDownClosed(object sender, EventArgs e)
    {
        foreach (ToolStripItem it in mProject.DropDownItems)
        {
            if (it is ToolStripMenuItem) it.Enabled = true;
        }
    }

    void ProjectAdd_Click(Object sender, EventArgs e)
    {
        AngleTabPage st;
        if (ZombieDocument || (st = (AngleTabPage)AngleTabControl.SelectedTab) != null && st.ContinRuns) return;

        OpenFileDialog ofd = new OpenFileDialog();
        ofd.InitialDirectory = InitialDirectory;
        ofd.DefaultExt = AdditionalFileDropExtensions[0];
        ofd.Multiselect = true;

        string exts = string.Format("*{0}", AdditionalFileDropExtensions[0]);
        for (int i = 1; i < AdditionalFileDropExtensions.Length; i++)
        {
            exts += string.Format("; *{0}", AdditionalFileDropExtensions[i]);
        }
        ofd.Filter = string.Format("ALV Data Files ({0})|{0}|All files (*.*)|*.*", exts);


        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            foreach (string file in ofd.FileNames)
            {
                if (ZombieDocument) FileNew_Click(this, EventArgs.Empty);
                if (AddDataFile(file, true) != null)
                {
                    Changed = true;
                    if (Untitled) InitialDirectory = Path.GetDirectoryName(file);
                }
            }

            foreach (AngleTabPage atp in AngleTabControl.TabPages)
            {
                atp.GetAllStats(true);
            }
        }
        ofd.Dispose();
    }

    protected override void OnDragOver(DragEventArgs drgevent)
    {
        //Dit is om te voorkomen dat dat-file gedropt worden op een zombie document

        base.OnDragOver(drgevent);
        if (drgevent.Data.GetDataPresent(DataFormats.FileDrop) && (drgevent.AllowedEffect & DragDropEffects.Copy) != 0)
        {
            string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(file);
                if (ZombieDocument && AllowedAdditionalExension(fi.Extension)) drgevent.Effect = DragDropEffects.None;
            }
        }
    }

    protected override void OnAdditionalFileExtensionDragDrop(FileNameEventArgs fileNameEventArgs)
    {
        base.OnAdditionalFileExtensionDragDrop(fileNameEventArgs);
        foreach (string file in fileNameEventArgs.FileNames)
        {
            if (ZombieDocument) FileNew_Click(this, EventArgs.Empty);
            if (AddDataFile(file, true) != null)
            {
                Changed = true;
                if (Untitled) InitialDirectory = Path.GetDirectoryName(file);
            }
        }

        foreach (AngleTabPage atp in AngleTabControl.TabPages)
        {
            atp.GetAllStats(true);
        }
    }

    private AngleTabPage AddDataFile()
    {
        return AddDataFile(double.NaN);
    }

    private AngleTabPage AddDataFile(double angle)
    {
        AngleTabPage atp = new AngleTabPage(angle);
        AngleTabControl.TabPages.Add(atp);
        atp.precontin.Selected += TabControlSelected;
        atp.precontin.SelectedIndexChanged += TabControlSelectedIndexChanged;
        atp.DataChanged += new EventHandler(atp_DataChanged);
        return atp;
    }

    private AngleTabPage AddDataFile(string file, bool check, string saveddatetime)
    {
        AngleTabPage atp = null;
        DataFile df = new DataFile(file);
        if (df.ReadData())
        {
            if (saveddatetime != null && saveddatetime != df.Datetime)
            {
                MessageBox.Show(this, string.Format("The date field in the data file differs from the date in the project file.\nThis could indicate that the wrong data file is being loaded!\n\nThe offending file {0} will be unchecked.\n\nDate in the data file: {1}\nDate in the project file: {2}", Path.GetFullPath(df.FileName), df.Datetime, saveddatetime), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (check)
                {
                    check = false;
                    Changed = true;
                }
            }

            if (AngleTabControl.TabPages.Count == 1 && ((AngleTabPage)AngleTabControl.TabPages[0]).Dummy)
                AngleTabControl.TabPages.Clear();

            foreach (AngleTabPage tp in AngleTabControl.TabPages)
            {
                if (Math.Abs(df.Angle - tp.AngleRounded) < Preferences.AngleTolerance)
                {
                    atp = tp;
                    break;
                }
            }
            //SuspendLayout();
            //AngleTabControl.SuspendLayout();

            if (atp == null)
            {
                atp = new AngleTabPage(df.Angle);
                AngleTabControl.TabPages.Add(atp);
                AngleTabControl.SelectedTab = atp;
                atp.precontin.Selected += TabControlSelected;
                atp.precontin.SelectedIndexChanged += TabControlSelectedIndexChanged;
                atp.DataChanged += new EventHandler(atp_DataChanged);
            }

            atp.AddDataFile(df, check);

            AngleTabControl.ResumeLayout();
            ResumeLayout();

            return atp;
        }
        else
        {
            MessageBox.Show(this, df.ErrorMessage, string.Format("Error in {0}", file), MessageBoxButtons.OK);
            return null;
        }
    }

    private AngleTabPage AddDataFile(string file, bool check)
    {
        return AddDataFile(file, check, null);
    }

    void atp_DataChanged(object sender, EventArgs e)
    {
        Changed = true;
    }

    void ProjectRemove_Click(object sender, EventArgs e)
    {
        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null && !atp.ContinRuns && atp.listviewdatafiles.Enabled && atp.listviewdatafiles.SelectedItems.Count > 0) atp.remove_click(this, EventArgs.Empty);
    }

    void ProjectExportInputFile_Click(object sender, EventArgs e)
    {
        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null && !atp.ContinRuns && atp.listviewdatafiles.CheckedItems.Count > 0) atp.exportcontininputfile(this, EventArgs.Empty);
    }

    void mGraph_DropDownOpening(object sender, EventArgs e)
    {
        Plotter p = null;
        mGraphChangeColor.Enabled = false;
        mGraphPrint.Enabled = false;


        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        if (atp != null)
        {
            if (atp.plotCountRates.ContainsFocus) p = atp.plotCountRates;
            if (atp.plotCorrelations.ContainsFocus) p = atp.plotCorrelations;
            if (atp.plotFit.ContainsFocus) p = atp.plotFit;
            if (atp.plotTransform.ContainsFocus) p = atp.plotTransform;
            if (p != null)
            {
                mGraphPrint.Enabled = true;
                if (p.Graphs.SelectedGraph != null) mGraphChangeColor.Enabled = true;
            }
        }
    }

    void mGraph_DropDownClosed(object sender, EventArgs e)
    {
        foreach (ToolStripItem it in mProject.DropDownItems)
        {
            if (it is ToolStripMenuItem) it.Enabled = true;
        }
    }

    void GraphChangeColor_Click(Object sender, EventArgs e)
    {
        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        Plotter p = null;

        if (ZombieDocument) return;
        if (atp != null)
        {
            if (atp.plotCountRates.ContainsFocus) p = atp.plotCountRates;
            if (atp.plotCorrelations.ContainsFocus) p = atp.plotCorrelations;
            if (atp.plotFit.ContainsFocus) p = atp.plotFit;
            if (atp.plotTransform.ContainsFocus) p = atp.plotTransform;
            if (p != null)
            {
                if (p.Graphs.SelectedGraph != null)
                {
                    p.SetColor(sender, e);
                }
            }
        }
    }

    void GraphPrint_Click(Object sender, EventArgs e)
    {
        AngleTabPage atp = (AngleTabPage)AngleTabControl.SelectedTab;
        Plotter p = null;

        if (ZombieDocument) return;
        if (atp != null)
        {
            if (atp.plotCountRates.ContainsFocus) p = atp.plotCountRates;
            if (atp.plotCorrelations.ContainsFocus) p = atp.plotCorrelations;
            if (atp.plotFit.ContainsFocus) p = atp.plotFit;
            if (atp.plotTransform.ContainsFocus) p = atp.plotTransform;
            if (p != null)
            {
                FilePrintGraph_Click(sender, e);
            }
        }
    }

    void printproject_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
    {
        Graphics g = e.Graphics;
        Font font = new Font(Font, FontStyle.Regular);
        Font fontbold = new Font(font, FontStyle.Bold);
        SolidBrush brush = new SolidBrush(Color.Black);
        System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
        StringFormat AlignFar = new StringFormat();
        AlignFar.Alignment = StringAlignment.Far;
        bool pagefits = false;

        int endingprinttab = AngleTabControl.TabPages.Count;
        while (!pagefits)
        {
            pagefits = true;

            g.DrawRectangle(new Pen(Color.Black), e.MarginBounds);

            float xloc = e.MarginBounds.Left + 5;
            float yloc = e.MarginBounds.Top + 5;
            float yspacing = 1.5f * font.Height;
            float tab = 7.4f * font.Height;

            g.DrawString(string.Format("Project {0}{1}", Path.GetFileNameWithoutExtension(DocumentName), startingprinttab > 0 ? " (continued)" : ""), fontbold, brush, xloc, yloc);
            g.DrawString(string.Format("{0}", info.Comments), fontbold, brush, e.MarginBounds.Right - 5, yloc, AlignFar);
            yloc += yspacing;
            g.DrawString(string.Format("({0})", Untitled ? "unsaved" : DocumentName), font, brush, xloc, yloc);
            g.DrawString(string.Format("{0}.{1} (Build {2})", info.ProductMajorPart, info.ProductMinorPart, info.ProductBuildPart), fontbold, brush, e.MarginBounds.Right - 5, yloc, AlignFar);
            yloc += yspacing;

            for (int i = startingprinttab; i < endingprinttab; i++)
            {
                AngleTabPage tp = AngleTabControl.TabPages[i] as AngleTabPage;
                Dullware.Library.Worksheet ws = new Dullware.Library.Worksheet(100, 200);
                yloc += yspacing;
                g.DrawString(string.Format("TabPage {0}", tp.Text), fontbold, brush, xloc, yloc);
                g.DrawLine(new Pen(Color.Black), xloc, yloc, e.MarginBounds.Right - 5, yloc);
                yloc += yspacing;
                if (tp.listviewdatafiles.Items.Count > 0)
                {
                    //            	g.DrawString("Average",font,brush,xloc+1*tab,yloc);
                    //            	g.DrawString("Deviation",font,brush,xloc+2*tab,yloc);
                    //            	yloc += yspacing;
                    //            	g.DrawString("Angle (deg)",font,brush,xloc+0*tab,yloc);
                    //            	g.DrawString(tp.anglestats.Average,font,brush,xloc+1*tab,yloc);
                    //            	g.DrawString(tp.anglestats.Deviation,font,brush,xloc+2*tab,yloc);
                    //            	yloc += yspacing;
                    //            	g.DrawString("Temperature (K)",font,brush,xloc+0*tab,yloc);
                    //            	g.DrawString(tp.temperaturestats.Average,font,brush,xloc+1*tab,yloc);
                    //            	g.DrawString(tp.temperaturestats.Deviation,font,brush,xloc+2*tab,yloc);
                    //            	yloc += yspacing;
                    //            	g.DrawString("Viscosity (mPI)",font,brush,xloc+0*tab,yloc);
                    //            	g.DrawString(tp.viscositystats.Average,font,brush,xloc+1*tab,yloc);
                    //            	g.DrawString(tp.viscositystats.Deviation,font,brush,xloc+2*tab,yloc);
                    //            	yloc += yspacing;
                    //            	g.DrawString("Refractive index",font,brush,xloc+0*tab,yloc);
                    //            	g.DrawString(tp.refractiveindexstats.Average,font,brush,xloc+1*tab,yloc);
                    //            	g.DrawString(tp.refractiveindexstats.Deviation,font,brush,xloc+2*tab,yloc);
                    //            	yloc += yspacing;

                    ws.Add(1, 0, "Average"); ws.Add(2, 0, "Deviation"); ws.YPos++;
                    ws.Add("Angle (deg)"); ws.Add(1, 0, tp.anglestats.Average); ws.Add(2, 0, tp.anglestats.Deviation); ws.YPos++;
                    ws.Add("Temperature (K)"); ws.Add(1, 0, tp.temperaturestats.Average); ws.Add(2, 0, tp.temperaturestats.Deviation); ws.YPos++;
                    ws.Add("Viscosity (mPI)"); ws.Add(1, 0, tp.viscositystats.Average); ws.Add(2, 0, tp.viscositystats.Deviation); ws.YPos++;
                    ws.Add("Refractive index"); ws.Add(1, 0, tp.refractiveindexstats.Average); ws.Add(2, 0, tp.refractiveindexstats.Deviation); ws.YPos++;
                    ws.YPos++;
                    ws.Add("Lower bound"); ws.Add(1, 0, tp.lslider.Index.ToString()); ws.YPos++;
                    ws.Add("Upper bound"); ws.Add(1, 0, tp.rslider.Index.ToString()); ws.YPos++;
                    ws.YPos++;

                    ws.XPos = 3; ws.YPos = 0;
                    ws.Add("Contin output"); ws.YPos++;
                    ws.Add(0, 0, "Gamma (s^-1)");
                    ws.Add(1, 0, "D (m^2 s^-1)");
                    ws.Add(2, 0, "R (nm)");
                    ws.Add(3, 0, "Area"); ws.YPos++;
                    foreach (ListViewItem item in tp.ContinOutputOverwiew.Items)
                    {
                        ws.Add(0, 0, item.SubItems[0].Text);
                        ws.Add(1, 0, item.SubItems[1].Text);
                        ws.Add(2, 0, item.SubItems[2].Text);
                        ws.Add(3, 0, item.SubItems[3].Text);
                        ws.YPos++;
                    }

                    ws.XPos = 0;
                    ws.YPos = ws.YSize + 1;
                    ws.Add("Included Datafile(s):"); ws.YPos++;
                    foreach (DataFile df in tp.listviewdatafiles.Items)
                    {
                        ws.Add(string.Format("{0} {1}", df.FileName, df.Checked ? "(checked)" : "(unchecked)"));
                        ws.YPos++;
                    }

                    ws.DrawToGraphics(g, font, brush, xloc, yloc, tab, yspacing);
                    yloc += ws.YSize * yspacing;
                    if (yloc > e.MarginBounds.Bottom)
                    {
                        pagefits = false;
                        endingprinttab--;
                        g.Clear(Color.White);
                    }
                }
                else g.DrawString("No Data", font, brush, xloc, yloc);
            }
        }
        if (endingprinttab < AngleTabControl.TabPages.Count)
        {
            e.HasMorePages = true;
            startingprinttab = endingprinttab;
        }
    }

    void ProjectExportXML_Click(object sender, EventArgs e)
    {
    	ProjectExport(true);
    }

    void ProjectExportCSV_Click(object sender, EventArgs e)
    {
    	ProjectExport(false);
    }

    void ProjectExport(bool xml)
    {
        AngleTabPage st;
        if (ZombieDocument || (st = (AngleTabPage)AngleTabControl.SelectedTab) != null && (st.ContinRuns || st.listviewdatafiles.Items.Count == 0)) return;

        string exported_files = null;

        if (Untitled)
        {
            MessageBox.Show(this, "Please save your untitled project prior to exporting data.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        if (!xml && MessageBox.Show(this, "Any existing export file will be overwritten.\n\nProceed?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            return;

        Workbook wbook = new Workbook();
        CarlosAg.ExcelXmlWriter.Worksheet wsheet;
        foreach (AngleTabPage tp in AngleTabControl.TabPages)
        {
            StreamWriter SW = null;
            if ( !xml ) 
            {
	            try
	            {
	                string filename = string.Format("{0}-{1}.csv", Path.GetFileNameWithoutExtension(DocumentName), tp.Text);
	                SW = new StreamWriter(filename, false, System.Text.Encoding.ASCII);
	                exported_files += string.Format("\n{0}", filename);
	            }
	            catch (Exception ep)
	            {
	                MessageBox.Show(this, ep.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	                continue;
	            }
            }
			wsheet = wbook.Worksheets.Add(tp.Text);
			
            Dullware.Library.Worksheet ws = new Dullware.Library.Worksheet(100, 50);
            ws.Add(string.Format("Project {0}", DocumentName));
            ws.Add(1, 0, string.Format("TabPage {0}", tp.Text)); ws.YPos++;

            if (tp.listviewdatafiles.Items.Count > 0)
            {
                ws.Add(1, 0, "Average"); ws.Add(2, 0, "Deviation"); ws.YPos++;
                ws.Add("Angle (deg)"); ws.Add(1, 0, tp.anglestats.Average); ws.Add(2, 0, tp.anglestats.Deviation); ws.YPos++;
                ws.Add("Temperature (K)"); ws.Add(1, 0, tp.temperaturestats.Average); ws.Add(2, 0, tp.temperaturestats.Deviation); ws.YPos++;
                ws.Add("Viscosity (mPI)"); ws.Add(1, 0, tp.viscositystats.Average); ws.Add(2, 0, tp.viscositystats.Deviation); ws.YPos++;
                ws.Add("Refractive index"); ws.Add(1, 0, tp.refractiveindexstats.Average); ws.Add(2, 0, tp.refractiveindexstats.Deviation); ws.YPos++;
                ws.YPos++;
                ws.Add("Lower bound"); ws.Add(1, 0, tp.lslider.Index.ToString()); ws.YPos++;
                ws.Add("Upper bound"); ws.Add(1, 0, tp.rslider.Index.ToString()); ws.YPos++;
                ws.YPos++;

                ws.XPos = 4; ws.YPos = 0;
                ws.Add("Contin output"); ws.YPos++;
                ws.Add(0, 0, "Gamma (s^-1)");
                ws.Add(1, 0, "D (m^2 s^-1");
                ws.Add(2, 0, "R (nm)");
                ws.Add(3, 0, "Area"); ws.YPos++;
                foreach (ListViewItem item in tp.ContinOutputOverwiew.Items)
                {
                    ws.Add(0, 0, item.SubItems[0].Text);
                    ws.Add(1, 0, item.SubItems[1].Text);
                    ws.Add(2, 0, item.SubItems[2].Text);
                    ws.Add(3, 0, item.SubItems[3].Text);
                    ws.YPos++;
                }
                ws.XPos = 0;
                ws.YPos = ws.YSize + 1;

                ws.Add("Correlations"); ws.YPos++;
                ws.Add("index"); for (int i = 0; i < ((DataFile)tp.listviewdatafiles.Items[0]).Correlations[0].Length; i++) ws.Add(0, i + 2, i.ToString());
                ws.XPos++;
                ws.Add("t(s)"); ws.Add(0, 2, ((DataFile)tp.listviewdatafiles.Items[0]).Correlations[0].X, Dullware.Library.Orientation.Down);
                ws.XPos++;

                foreach (DataFile df in tp.listviewdatafiles.Items)
                {
                    ws.Add(Path.GetFileNameWithoutExtension(df.FileName));
                    ws.Add(0, 1, df.Checked ? "checked" : "unchecked");
                    foreach (DataSet ds in df.Correlations)
                    {
                        ws.Add(0, 2, ds.Y, Dullware.Library.Orientation.Down);
                        ws.XPos++;
                    }
                }
                if (tp.plotFit.Graphs["average"].DSet != null)
                {
                    ws.Add("Average");
                    ws.Add(0, 2, tp.plotFit.Graphs["average"].DSet.Y, Dullware.Library.Orientation.Down);
                    ws.XPos++;
                }

                if (tp.plotFit.Graphs["fit"].DSet != null)
                {
                    ws.Add("Fit");
                    ws.Add(0, 2 + tp.lslider.Index, tp.plotFit.Graphs["fit"].DSet.Y, Dullware.Library.Orientation.Down);
                    ws.XPos++;
                }
                ws.XPos = 0;
                ws.YPos = ws.YSize + 1;

                ws.Add("Count rates (kHz)"); ws.YPos++;
                ws.Add("index"); for (int i = 0; i < ((DataFile)tp.listviewdatafiles.Items[0]).CountRates[0].Length; i++) ws.Add(0, i + 2, i.ToString());
                ws.XPos++;
                ws.Add("t(s)"); ws.Add(0, 2, ((DataFile)tp.listviewdatafiles.Items[0]).CountRates[0].X, Dullware.Library.Orientation.Down);
                ws.XPos++;

                foreach (DataFile df in tp.listviewdatafiles.Items)
                {
                    ws.Add(Path.GetFileNameWithoutExtension(df.FileName));
                    ws.Add(0, 1, df.Checked ? "checked" : "unchecked");
                    foreach (DataSet ds in df.CountRates)
                    {
                        ws.Add(0, 2, ds.Y, Dullware.Library.Orientation.Down);
                        ws.XPos++;
                    }
                }
                ws.XPos = 0;
                ws.YPos = ws.YSize + 1;

                if (tp.plotTransform.Graphs.Count > 0)
                {
                    DataSet tform = ((PlotGraph)tp.plotTransform.Graphs[tp.plotTransform.Graphs.Count - 1]).DSet;
                    ws.Add("Transform"); ws.YPos++;
                    ws.Add("index"); for (int i = 0; i < tform.Length; i++) ws.Add(0, i + 1, i.ToString()); ws.XPos++;
                    ws.Add("Gamma (s^-1)"); for (int i = 0; i < tform.Length; i++) ws.Add(0, i + 1, tform.X[i]); ws.XPos++;
                    ws.Add("D (um^2/s)"); for (int i = 0; i < tform.Length; i++) ws.Add(0, i + 1, 1e12 * tp.DiffusionConstant(tform.X[i])); ws.XPos++;
                    ws.Add("R (nm)"); for (int i = 0; i < tform.Length; i++) ws.Add(0, i + 1, 1e9 * tp.HydrodynamicRadius(tform.X[i])); ws.XPos++;
                    ws.Add("Gamma * Weight (s^-1)"); for (int i = 0; i < tform.Length; i++) ws.Add(0, i + 1, tform.Y[i]); ws.XPos++;
                }
                ws.XPos = 0;
                ws.YPos = ws.YSize + 1;
            }
            else ws.Add("No data");

            if ( xml ) ws.SaveAsXmlSheet(wsheet);
            else
            {
	            ws.SaveInCSV(SW);
	            SW.Close();
            }
        }
        
        if ( xml )
        	SaveXmlAs(wbook,string.Format("{0}.xml", Path.GetFileNameWithoutExtension(DocumentName)));
        else
        {
	        if (exported_files != null)
	            MessageBox.Show(this, string.Format("The following files are written to {0}:\n{1}", Environment.CurrentDirectory, exported_files), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
	        else
	            MessageBox.Show(this, string.Format("No files are written\n"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SaveXmlAs(Workbook wb,string fname)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.InitialDirectory = InitialDirectory;
        sfd.DefaultExt = "xml";
        sfd.FileName = fname;
        sfd.Filter = "XML Spreadsheet (*.xml)|*.xml|All files (*.*)|*.*";

        bool result = sfd.ShowDialog(this) == DialogResult.OK;

        if (result)
        {
            FileNameEventArgs fnea = new FileNameEventArgs(sfd.FileName);
            OnWriteRequest(fnea);
        	try
        	{
         		wb.Save(sfd.FileName);
                InitialDirectory = System.IO.Path.GetDirectoryName(sfd.FileName);
       		}
            catch/*(Exception ep)*/
            {
            	//fouten worden in excelxmlwriter afgevangen
                //MessageBox.Show(this, ep.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        sfd.Dispose();
    }

        protected override void OnReadRequest(FileNameEventArgs e)
    {
        base.OnReadRequest(e);
        bool saveafterreading = false;
        AngleTabPage atp = null;
        string savedfilename = null;
        char splitchar = '/'; //Het oude formaat: wordt alleen nog voor de eerste regel gebruikt

        Cursor = Cursors.WaitCursor;
        StreamReader SR = null;
        try
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(e.FileName));
            SR = new StreamReader(e.FileName, System.Text.Encoding.ASCII);
            string s = SR.ReadLine();
            while (s != null)
            {
                if (s.StartsWith("#")) continue;

                string[] args = s.Split(splitchar);
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "FormatVersion":
                            splitchar = '|'; //de standaard vanaf 1.3
                            switch (args[1])
                            {
                                case "1.0":
                                case "1.1":
                                    SR.Close();
                                    Cursor = DefaultCursor;
                                    MessageBox.Show(this, "This ancient project file is no longer supported.\nThe project file will be deleted.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    File.Delete(e.FileName);
                                    e.Cancel = true;
                                    return;
                                case "1.2":
                                    splitchar = '/'; //Geeft problemen met Date format
                                    break;
                                case "1.3":
                                    break;
                                default:
                                    MessageBox.Show(this, "This project file was created by a newer version of AfterALV.\nBe careful to overwrite is project file, as this may reduce features!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    break;
                            }
                            break;
                        case "FileName":
                            savedfilename = args[1];
                            if (savedfilename != e.FileName)
                            {
                                saveafterreading = true;
                                Changed = true;
                            }
                            break;
                        case "AngleTolerance":
                            //Voor de toekomst.
                            break;
                        case "TabPage":
                            break;
                        case "DataFiles":
                            int nfiles = int.Parse(args[1]);
                            for (int i = 0; i < nfiles; i++)
                            {
                                s = SR.ReadLine();
                                args = s.Split(splitchar);
                                string datafile = TestFileName(args[1], savedfilename);
                                if (args[1] != datafile) saveafterreading = true;
                                atp = AddDataFile(datafile, bool.Parse(args[2]), args[3]);
                            }
                            break;
                        case "Bounds":
                            if (atp != null)
                            {
                                atp.lslider.Index = int.Parse(args[1]);
                                atp.rslider.Index = int.Parse(args[2]);
                            }
                            break;
                        default:
                            break;
                    }
                }
                s = SR.ReadLine();
            }
            SR.Close();

            foreach (AngleTabPage angle in AngleTabControl.TabPages)
            {
                angle.GetAllStats(true);
            }

            if (saveafterreading)
            {
                if (MessageBox.Show(this, string.Format("The project file {0} needs updating because it was renamed, moved, or otherwise tempered with.\n\nDo you want to update the project file?", e.FileName), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    FileSaveAs_Click(this, new EventArgs());
                //MessageBox.Show(this, string.Format("The project file {0} needs updating because its location, or the location of one or more of its data files, was changed.\n\nPlease save the project file at your earliest convenience.", e.FileName), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (AngleTabControl.TabPages.Count == 0) AddDataFile();
        }
        catch (Exception exc)
        {
            if (SR != null) SR.Close();
            e.Cancel = true;
            MessageBox.Show(this, string.Format("Error reading project file {0}:\n\n{1}", e.FileName, exc.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Cursor = DefaultCursor;
    }

    private string TestFileName(string datafile, string projectfile)
    {
        // We zijn al in de dir van de project file.
        string retpath = datafile;
        if (!(new FileInfo(datafile)).Exists) //kan beter met File.Exists
        {
            if (!Win32API.PathRelativePathTo(ref retpath, projectfile, datafile)) retpath = datafile;
            retpath = Path.GetFullPath(retpath);
            if (!(new FileInfo(retpath)).Exists) retpath = datafile;
        }
        if (retpath != datafile) Changed = true;
        return retpath;
    }

    protected override void OnWriteRequest(FileNameEventArgs e)
    {
        base.OnWriteRequest(e);
        char splitchar = '|';

        StreamWriter SW = null;
        try
        {
            SW = new StreamWriter(e.FileName, false, System.Text.Encoding.ASCII);
            SW.WriteLine("FormatVersion{0}{1}", '/', "1.3"); //eerste regel heeft splitchar '/'.
            SW.WriteLine("FileName{0}{1}", splitchar, e.FileName);
            SW.WriteLine("AngleTolerance{0}{1}", splitchar, Preferences.AngleTolerance);
            foreach (AngleTabPage tp in AngleTabControl.TabPages)
            {
                if (tp.listviewdatafiles.Items.Count > 0)
                {
                    SW.WriteLine("TabPage{0}{1}{0}{2}", splitchar, AngleTabControl.TabPages.IndexOf(tp), tp.Text);
                    SW.WriteLine("DataFiles{0}{1}", splitchar, tp.listviewdatafiles.Items.Count);
                    foreach (DataFile df in tp.listviewdatafiles.Items)
                    {
                        SW.WriteLine("DataFile{0}{1}{0}{2}{0}{3}", splitchar, df.FileName, df.Checked, df.Datetime);
                    }
                    SW.WriteLine("Bounds{0}{1}{0}{2}", splitchar, tp.lslider.Index, tp.rslider.Index);
                }
            }
            SW.Close();
        }
        catch (Exception exc)
        {
            if (SW != null) SW.Close();
            e.Cancel = true;
            MessageBox.Show(this, string.Format("Error writing project file {0}:\n\n{1}", e.FileName, exc.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void TabControlSelected(object o, TabControlEventArgs e)
    {
        foreach (AngleTabPage atp in AngleTabControl.TabPages)
            atp.precontin.SelectedIndex = e.TabPageIndex;
    }

    void TabControlSelectedIndexChanged(object o, EventArgs e)
    {
        foreach (AngleTabPage atp in AngleTabControl.TabPages)
            atp.precontin.SelectedTab.Controls[0].Focus();
    }
}
