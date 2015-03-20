using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using Dullware.Plotter;

class AngleTabPage : TabPage
{
    readonly double k = 1.3806505e-23;

    public Button bRunContin;

    private string last_exportcontininputfile = null;

    public ListView listviewdatafiles;
    Timer listviewdatafilescheckedtimer;
    bool ignorecheckedchanged;
    bool getstatspending;

    public bool Dummy
    {
        get { return double.IsNaN(anglerounded); }
    }

    double anglerounded;
    public double AngleRounded
    {
        get { return anglerounded; }
        //set { roundedangle = value; }
    }

    public StatData anglestats = new StatData();
    public StatData temperaturestats = new StatData();
    public StatData viscositystats = new StatData();
    public StatData refractiveindexstats = new StatData();
    StatData correlationstats = new StatData();

    public TabControl precontin = new TabControl();

    public Plotter plotCountRates;
    public Plotter plotCorrelations;
    public Plotter plotFit;
    public Plotter plotTransform;


    public PlotSlider lslider = new PlotSlider();
    public PlotSlider rslider = new PlotSlider();

    DataSet av_correlations;


    BackgroundWorker bgw = new BackgroundWorker();

    public ListView ContinOutputOverwiew;
    public ListView History;
    double wavelength = double.NaN; //getal wordt uit een datafile gehaald.
    public double WaveNumber
    {
        get
        {
            return 4 * Math.PI * refractiveindexstats.Value / (wavelength / 1e9) * Math.Sin(anglestats.Value * Math.PI / 360d);
        }
    }

    public double DiffusionConstant(double gamma)
    {
        return gamma / Math.Pow(WaveNumber, 2);
    }

    public double HydrodynamicRadius(double gamma)
    {
        return k * temperaturestats.Value / (6d * Math.PI * viscositystats.Value / 1000d * DiffusionConstant(gamma));
        //delen door 1000 vanwege centipoise.
        //1 cP = 1 mPI = 0.001 Pa s (P = Poise, PI = Poisseuille)
    }

    bool continruns = false;
    public bool ContinRuns
    {
        get { return continruns; }
    }

    SplitterPanel infopanel;
    public AngleTabPage(double angle)
    {
        //SuspendLayout();

        anglerounded = Math.Round(angle / Preferences.AngleTolerance) * Preferences.AngleTolerance;
        if (!double.IsNaN(angle)) Text = string.Format("{0}", anglerounded);

        SplitContainer split1 = new SplitContainer();
        split1.Parent = this;
        split1.Dock = DockStyle.Fill;
        split1.BackColor = SystemColors.Control;
        split1.SplitterDistance = (int)(1.75 / 3 * split1.Width);
        infopanel = split1.Panel2;
        infopanel.BackColor = SystemColors.Window;
        //infopanel.SuspendLayout();

        SplitContainer split2 = new SplitContainer();
        split2.Parent = split1.Panel1;
        split2.Orientation = Orientation.Horizontal;
        split2.Dock = DockStyle.Fill;
        split2.BackColor = SystemColors.Control;
        split2.SplitterDistance = (int)(4d / 5 * split1.Height);
        SplitterPanel listpanel = split2.Panel2;

        SplitContainer split3 = new SplitContainer();
        split3.Parent = split2.Panel1;
        split3.Orientation = Orientation.Horizontal;
        split3.Dock = DockStyle.Fill;
        split3.BackColor = SystemColors.Control;
        SplitterPanel transformpanel = split3.Panel1;
        SplitterPanel plotspanel = split3.Panel2;

        SplitterPanel flow = infopanel;
        //FlowLayoutPanel flow = new FlowLayoutPanel();
        //flow.SuspendLayout();
        //flow.Parent = infopanel;
        //flow.Padding = new Padding(Font.Height);
        //flow.AutoSize = true;

        int xoffset = 5;
        int yoffset = 5;
        int xsize;
        int ysize;

        Label lb;
        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "refractive index   ";
        xsize = lb.Width;
        ysize = lb.Height;
        lb.Parent = null;
        //split1.SplitterDistance = 540 - (xoffset + 3 * xsize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "";

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "Average";
        //lb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        lb.Location = new Point(xoffset + 1 * xsize, yoffset + 0 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "Deviation";
        lb.Location = new Point(xoffset + 2 * xsize, yoffset + 0 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "angle (deg)";
        lb.Location = new Point(xoffset + 0 * xsize, yoffset + 1 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", anglestats, "Average");
        lb.Location = new Point(xoffset + 1 * xsize, yoffset + 1 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", anglestats, "Deviation");
        lb.Location = new Point(xoffset + 2 * xsize, yoffset + 1 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "temperature (K)";
        lb.Location = new Point(xoffset + 0 * xsize, yoffset + 2 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", temperaturestats, "Average");
        lb.Location = new Point(xoffset + 1 * xsize, yoffset + 2 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", temperaturestats, "Deviation");
        lb.Location = new Point(xoffset + 2 * xsize, yoffset + 2 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "viscosity (mPI)";
        lb.Location = new Point(xoffset + 0 * xsize, yoffset + 3 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", viscositystats, "Average");
        lb.Location = new Point(xoffset + 1 * xsize, yoffset + 3 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", viscositystats, "Deviation");
        lb.Location = new Point(xoffset + 2 * xsize, yoffset + 3 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "refractive index";
        lb.Location = new Point(xoffset + 0 * xsize, yoffset + 4 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", refractiveindexstats, "Average");
        lb.Location = new Point(xoffset + 1 * xsize, yoffset + 4 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", refractiveindexstats, "Deviation");
        lb.Location = new Point(xoffset + 2 * xsize, yoffset + 4 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.Text = "correlations";
        lb.Location = new Point(xoffset + 0 * xsize, yoffset + 5 * ysize);

        lb = new Label();
        lb.Parent = flow;
        lb.AutoSize = true;
        lb.DataBindings.Add("Text", correlationstats, "Deviation");
        lb.Location = new Point(xoffset + 2 * xsize, yoffset + 5 * ysize);

        bRunContin = new Button();
        //bRunContin.Parent = flow;
        bRunContin.AutoSize = true;
        bRunContin.Text = "Run Contin";
        bRunContin.Location = new Point(xoffset + 0 * xsize, yoffset + 7 * ysize);
        bRunContin.Click += RunContinClick;

        History = new ListView();
        History.Parent = flow;
        History.Location = new Point(xoffset + 0 * xsize, yoffset + 10 * ysize);
        History.Dock = DockStyle.Bottom;
        History.FullRowSelect = true;
        History.MultiSelect = false;
        History.Scrollable = true;
        History.View = View.Details;
        History.Sorting = SortOrder.None;
        History.HideSelection = false;
        History.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(History_ItemSelectionChanged);

        History.Columns.Add("Interval", 70);
        History.Columns.Add("Checked", 70);

        History.ContextMenuStrip = new ContextMenuStrip();
        History.ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
        History.ContextMenuStrip.Items.Add("Keep this one, and purge the rest", null, PurgeHistory);

        ContinOutputOverwiew = new ListView();
        ContinOutputOverwiew.Parent = flow;
        ContinOutputOverwiew.Location = new Point(xoffset + 0 * xsize, yoffset + 10 * ysize);
        ContinOutputOverwiew.Dock = DockStyle.Bottom;
        ContinOutputOverwiew.FullRowSelect = true;
        ContinOutputOverwiew.Scrollable = true;
        ContinOutputOverwiew.View = View.Details;
        ContinOutputOverwiew.Sorting = SortOrder.None;
        ContinOutputOverwiew.HideSelection = false;

        ContinOutputOverwiew.Columns.Add("Gamma (s^-1)", 100);
        ContinOutputOverwiew.Columns.Add("D   (um^2/s)", 100);
        ContinOutputOverwiew.Columns.Add("R       (nm)", 100);
        ContinOutputOverwiew.Columns.Add("Area        ", 100);


        listviewdatafiles = new ListView();
        listviewdatafiles.Parent = listpanel;
        listviewdatafiles.Dock = DockStyle.Fill;
        listviewdatafiles.CheckBoxes = true;
        listviewdatafiles.FullRowSelect = true;
        listviewdatafiles.MultiSelect = true;
        listviewdatafiles.Scrollable = true;
        listviewdatafiles.View = View.Details;
        listviewdatafiles.Sorting = SortOrder.Ascending;
        listviewdatafiles.HideSelection = false;
        listviewdatafiles.Activation = ItemActivation.OneClick;
        listviewdatafiles.ShowItemToolTips = true;

        listviewdatafiles.Columns.Add("Filename", 100);
        listviewdatafiles.Columns.Add(@"#", 30);
        listviewdatafiles.Columns.Add("Sample", 150);
        listviewdatafiles.Columns.Add("Date", 150);
        listviewdatafiles.Columns.Add("Angle (deg)", 80);
        listviewdatafiles.Columns.Add("Temperature (K)", 80);
        listviewdatafiles.Columns.Add("Viscosity (mPI)", 80);
        listviewdatafiles.Columns.Add("Refr.index (-)", 80);

        listviewdatafilescheckedtimer = new Timer();
        listviewdatafilescheckedtimer.Tick += new EventHandler(listviewdatafilescheckedtimer_Tick);
        listviewdatafilescheckedtimer.Interval = 200;

        listviewdatafiles.ItemChecked += ItemCheckedChanged;
        listviewdatafiles.ItemSelectionChanged += ItemSelectionChanged;

        precontin.Parent = plotspanel;
        precontin.Dock = DockStyle.Fill;
        precontin.Alignment = TabAlignment.Bottom;


        TabPage tp;
        tp = new TabPage("Fit");
        plotFit = new Plotter("Fit");
        plotFit.Parent = tp;
        plotFit.Dock = DockStyle.Fill;
        plotFit.XScale = Scales.Logarithmic;
        plotFit.Caption = "Contin fit";
        plotFit.CaptionX = "tau (s)";
        plotFit.CaptionY = "g1^2";
        precontin.TabPages.Add(tp);

        tp = new TabPage("Correlations");
        plotCorrelations = new Plotter("Correlations");
        plotCorrelations.Parent = tp;
        plotCorrelations.Dock = DockStyle.Fill;
        plotCorrelations.XScale = Scales.Logarithmic;
        plotCorrelations.Graphs.Add("average");
        plotCorrelations.Graphs["average"].Pen = new Pen(Color.YellowGreen, 2);
        plotCorrelations.Caption = "Correlations";
        plotCorrelations.CaptionX = "tau (s)";
        plotCorrelations.CaptionY = "g1^2";
        precontin.TabPages.Add(tp);

        tp = new TabPage("Count rates");
        plotCountRates = new Plotter("Count Rates");
        plotCountRates.Parent = tp;
        plotCountRates.Dock = DockStyle.Fill;
        plotCountRates.Caption = "Count rates";
        plotCountRates.CaptionX = "time (s)";
        plotCountRates.CaptionY = "frequency (kHz)";
        precontin.TabPages.Add(tp);

        lslider.UpperLimitEnabled = true;
        rslider.LowerLimitEnabled = true;
        rslider.UpperLimitEnabled = true;
        lslider.LowerLimitEnabled = true;
        lslider.ValueChanged += lslider_ValueChanged;
        rslider.ValueChanged += rslider_ValueChanged;
        if (!double.IsNaN(angle))
        {
            plotFit.PlotSliders.Add(lslider);
            plotFit.PlotSliders.Add(rslider);
        }
        plotFit.Graphs.Add("average");
        plotFit.Graphs.Add("fit");
        //plotFit.Graphs["fit"].Pen = new Pen(Color.Tomato, 2);
        //plotFit.Graphs["average"].Pen = new Pen(Color.YellowGreen, 2);

        plotTransform = new Plotter("Transform");
        plotTransform.Parent = transformpanel;
        plotTransform.Dock = DockStyle.Fill;
        plotTransform.XScale = Scales.Logarithmic;
        plotTransform.Caption = "Inverse Laplace Transform";
        plotTransform.CaptionX = "gamma (s^-1)";
        plotTransform.CaptionY = "weight x gamma (s^-1)";

        bgw.DoWork += ContinWorker;
        bgw.RunWorkerCompleted += ContinWorkerCompleted;

        infopanel.ResumeLayout();
        flow.ResumeLayout();
        ResumeLayout();
    }

    void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        History.ContextMenuStrip.Items[0].Enabled = History.Items.Count > 1 && History.SelectedItems.Count > 0;
    }

    void PurgeHistory(object sender, EventArgs e)
    {
        if (History.Items[History.Items.Count - 1].Selected)
        { //De laatste is geselecteerd. De rest kan weg; contin hoeft niet te runnen.
            int limit = History.Items.Count - 1;
            for (int i = 0; i < limit; i++)
            {
                plotTransform.Graphs.Remove(((PlotGraph)plotTransform.Graphs[0]));
                History.Items[0].Remove();
            }
            ((PlotGraph)plotTransform.Graphs[0]).Selected = false;
            ((PlotGraph)plotTransform.Graphs[0]).PenColor = Color.FromArgb(plotTransform.ColorPalet[0]);
        }
        else
        {
            ListViewItem selitem = History.SelectedItems[0];
            plotTransform.Graphs.Clear();
            History.Items.Clear();
            ignorecheckedchanged = true;
            int i = 0;
            foreach (char c in selitem.SubItems[1].Text)
            {
                listviewdatafiles.Items[i++].Checked = c == 'Y';
            }
            ignorecheckedchanged = false;
            string[] splitted = selitem.Text.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
            lslider.Index = int.Parse(splitted[0]);
            rslider.Index = int.Parse(splitted[1]);
            GetAllStats(false);
            RunContinClick(null, EventArgs.Empty);
        }
    }

    void lslider_ValueChanged(object o, EventArgs e)
    {
        rslider.LowerLimitIndex = lslider.Index + 5;
        OnDataChanged(new EventArgs());
        RunContinClick(null, EventArgs.Empty);
    }

    void rslider_ValueChanged(object o, EventArgs e)
    {
        lslider.UpperLimitIndex = rslider.Index - 5;
        OnDataChanged(new EventArgs());
        RunContinClick(null, EventArgs.Empty);
    }

    void RunContinClick(object sender, EventArgs e)
    {
        continruns = true;
        bRunContin.Enabled = false;
        lslider.Enabled = false;
        rslider.Enabled = false;
        listviewdatafiles.Enabled = false;
        History.Enabled = false;

        plotFit.Select();
        Cursor = Cursors.WaitCursor;

        if (av_correlations == null) return;

        ContinInput cinput = new ContinInput(av_correlations, lslider.Index, rslider.Index);
        ContinOutput coutput = new ContinOutput(cinput.Correlations.Length, 100);
        ContinInterface100 cinterface = new ContinInterface100();
        ContinInfo info = new ContinInfo(cinput, coutput, cinterface);

        bgw.RunWorkerAsync(info);
    }

    void ContinWorker(object sender, DoWorkEventArgs e)
    {
        ContinInfo info = e.Argument as ContinInfo;
        //System.Threading.Thread.Sleep(3000);
        string error = info.CInterface.RunContin(info.CInput, info.COutput);
        e.Result = error == null ? e.Argument : error;
    }

    void ContinWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Result is ContinInfo)
        {
            ContinInfo info = e.Result as ContinInfo;
            ContinInput cinput = info.CInput;
            ContinOutput coutput = info.COutput;
            ContinInterface100 cinterface = info.CInterface;

            //Haal de x-coordinaten uit input correlaties omdat ze in de continoutput onnodig onnauwkeurig zijn.
            for (int i = cinput.Correlations.LowerBound; i <= cinput.Correlations.UpperBound; i++)
                coutput.Correlations.X[i - cinput.Correlations.LowerBound] = cinput.Correlations.X[i];

            plotFit.Graphs["fit"].DSet = coutput.Correlations;

            PlotGraph pg;
            plotTransform.Graphs.Add(pg = new PlotGraph(coutput.Transform));
            pg.SelectedChanged += new EventHandler(pg_SelectedChanged);

            for (int i = History.Columns.Count - 2; i < coutput.Peaks.Count; i++) History.Columns.Add(string.Format("R{0} (nm)", i + 1), 100);
            ListViewItem history = new ListViewItem(string.Format("({0}, {1})", cinput.Correlations.LowerBound, cinput.Correlations.UpperBound));
            string checkeditems = "";
            foreach (DataFile df in listviewdatafiles.Items)
            {
                checkeditems += df.Checked ? "Y" : "N";
            }
            history.SubItems.Add(checkeditems);
            history.Selected = true;

            ContinOutputOverwiew.Items.Clear();
            foreach (ContinPeak peak in coutput.Peaks)
            {
                ListViewItem item = new ListViewItem(string.Format("{0}", peak.Position));
                item.SubItems.Add(string.Format("{0:0.#####}", 1e12 * DiffusionConstant(peak.Position)));
                item.SubItems.Add(string.Format("{0:0.#####}", 1e9 * HydrodynamicRadius(peak.Position)));
                item.SubItems.Add(string.Format("{0:0.#####}", peak.Area));
                history.SubItems.Add(string.Format("{0:0.#####}", 1e9 * HydrodynamicRadius(peak.Position)));
                ContinOutputOverwiew.Items.Add(item);
            }

            History.Items.Add(history);
            ((PlotGraph)plotTransform.Graphs[plotTransform.Graphs.Count - 1]).Selected = false;

            if (History.Items.Count == 1 && plotTransform.ColorPalet.Count > 0)
                ((PlotGraph)plotTransform.Graphs[0]).PenColor = Color.FromArgb(plotTransform.ColorPalet[0]);

        }
        else // e.Result bevat de error message.
        {
            MessageBox.Show(this, string.Format("For some reason contin did not run:\n{0}.", e.Result), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        Cursor = DefaultCursor;
        bRunContin.Enabled = true;
        lslider.Enabled = true;
        rslider.Enabled = true;
        listviewdatafiles.Enabled = true;
        History.Enabled = true;
        continruns = false;
    }

    void pg_SelectedChanged(object sender, EventArgs e)
    {
        PlotGraph pg = sender as PlotGraph;
        if (pg.Selected && plotTransform.Graphs.IndexOf(pg) != -1 && !History.Items[plotTransform.Graphs.IndexOf(pg)].Selected)
        {
            History.Items[plotTransform.Graphs.LastIndexOf(pg)].Selected = true;
            History.Items[plotTransform.Graphs.LastIndexOf(pg)].EnsureVisible();
        }
    }

    class ContinInfo
    {
        public ContinInfo(ContinInput cinput, ContinOutput coutput, ContinInterface100 cinterface)
        {
            this.cinput = cinput;
            this.coutput = coutput;
            this.cinterface = cinterface;
        }

        ContinInput cinput;

        public ContinInput CInput
        {
            get
            {
                return cinput;
            }
        }
        ContinOutput coutput;

        public ContinOutput COutput
        {
            get
            {
                return coutput;
            }
        }
        ContinInterface100 cinterface;

        public ContinInterface100 CInterface
        {
            get
            {
                return cinterface;
            }
        }
    }

    void ItemSelectionChanged(object o, ListViewItemSelectionChangedEventArgs e)
    {
        DataFile df = e.Item as DataFile;
        foreach (DataSet ds in df.CountRates)
        {
//            if (e.IsSelected) plotCountRates.Graphs.Add(ds);
//            else plotCountRates.Graphs.Remove(ds);

			PlotGraph pg = plotCountRates.Graphs[ds.Name];
			
            if (e.IsSelected)
            {
                if (pg != null) pg.DSet = ds;
                else plotCountRates.Graphs.Add(ds);
            }
            else pg.DSet = null;
        }

        foreach (DataSet ds in df.Correlations)
        {
//            if (e.IsSelected) plotCorrelations.Graphs.Add(ds);
//            else plotCorrelations.Graphs.Remove(ds);

			PlotGraph pg = plotCorrelations.Graphs[ds.Name];
			
            if (e.IsSelected)
            {
                if (pg != null) pg.DSet = ds;
                else plotCorrelations.Graphs.Add(ds);
            }
            else pg.DSet = null;
        }
    }

    void History_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        ListView history = sender as ListView;
        ContinOutputOverwiew.Enabled = e.ItemIndex == history.Items.Count - 1 && e.IsSelected;
        ((PlotGraph)plotTransform.Graphs[e.ItemIndex]).Selected = e.IsSelected;
        listviewdatafiles.Enabled = e.ItemIndex == history.Items.Count - 1 && e.IsSelected;
        infopanel.BackColor = e.ItemIndex == history.Items.Count - 1 && e.IsSelected ? SystemColors.Window : SystemColors.Control;
        ((PlotGraph)plotFit.Graphs["fit"]).Ignored = e.ItemIndex != history.Items.Count - 1 || !e.IsSelected;
    }

    void listviewdatafilescheckedtimer_Tick(object sender, EventArgs e)
    {
        listviewdatafilescheckedtimer.Stop();
        GetAllStats(true);
        OnDataChanged(new EventArgs());
    }

    void ItemCheckedChanged(object o, ItemCheckedEventArgs e)
    {
        getstatspending = true;
        if (ignorecheckedchanged) return;
        listviewdatafilescheckedtimer.Start();
    }

    public event EventHandler DataChanged;
    private void OnDataChanged(EventArgs e)
    {
        if (DataChanged != null) DataChanged(this, e);
    }

    public void AddDataFile(DataFile df, bool check)
    {
        foreach (DataFile d in listviewdatafiles.Items)
        {
            if (d.FileName == df.FileName)
            {
                MessageBox.Show(this, string.Format("File {0} is already included in container {1}.\n\nSkipped.", df.FileName, Text), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                df = null;
                break;
            }
            else if (d.Correlations[0].X.Length != df.Correlations[0].X.Length) //testen op mode, datalengte, golflengte
            {
                MessageBox.Show(this, string.Format("Data size incompatibility encountered in container {3}: \n\nThis file: {1}\nFile(s) already loaded: {2}\n\nFile {0} is skipped.", df.FileName, df.Correlations[0].X.Length, d.Correlations[0].X.Length, Text), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                df = null;
                break;
            }
            else if (d.Mode != df.Mode) //testen op mode, datalengte, golflengte
            {
                MessageBox.Show(this, string.Format("Operation mode incompatibility encountered in container {3}: \n\nThis file: {1}\nFile(s) already loaded: {2}\n\nFile {0} is skipped.", df.FileName, df.Mode, d.Mode, Text), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                df = null;
                break;
            }
            else if (d.WaveLength != df.WaveLength) //testen op mode, datalengte, golflengte
            {
                MessageBox.Show(this, string.Format("Wavelength incompatibility in container {3}: \n\nThis file: {1}\nFile(s) already loaded: {2}\n\nFile {0} is skipped.", df.FileName, df.WaveLength, d.WaveLength, Text), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                df = null;
                break;
            }
        }

        if (df != null)
        {
            wavelength = df.WaveLength;
            ignorecheckedchanged = true;
            listviewdatafiles.Items.Add(df);
            df.Checked = check;
            ignorecheckedchanged = false;
            //df.Selected = true;
            listviewdatafiles.Focus();
            if (plotFit.SnapToGraph == null)
            {
                plotFit.SnapToGraph = ((DataFile)listviewdatafiles.Items[0]).Correlations[0];
                //((PlotSlider)plotFit.PlotSliders[0]).Value = ((DataFile)listviewdatafiles.Items[0]).Correlations[0].X[40];
                //((PlotSlider)plotFit.PlotSliders[1]).Value = ((DataFile)listviewdatafiles.Items[0]).Correlations[0].X[126];
                ((PlotSlider)plotFit.PlotSliders[0]).Value = plotFit.SnapToGraph.X[plotFit.SnapToGraph.LowerBound+2];
                ((PlotSlider)plotFit.PlotSliders[1]).Value = plotFit.SnapToGraph.X[plotFit.SnapToGraph.UpperBound-2];
                rslider.LowerLimitIndex = lslider.Index + 2;
                rslider.UpperLimitIndex = plotFit.SnapToGraph.UpperBound - 2;
                lslider.UpperLimitIndex = rslider.Index - 2;
                lslider.LowerLimitIndex = 2;
            }
            History.Items.Clear();
            plotTransform.Graphs.Clear();
        }
    }

    public void remove_click(object o, EventArgs e)
    {
        foreach (DataFile item in listviewdatafiles.SelectedItems)
        {
            item.Checked = false;
            listviewdatafiles.Items.Remove(item);
        }
        History.Items.Clear();
        plotTransform.Graphs.Clear();
    }

    public void exportcontininputfile(object o, EventArgs e)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.DefaultExt = ".e";

        sfd.FileName = last_exportcontininputfile == null ? string.Format("{0}-{1}", Path.GetFileNameWithoutExtension(((AfterALV)o).DocumentName), Text) : last_exportcontininputfile;

        sfd.Filter = "Contin Input Files (*.l)|*.l|All files (*.*)|*.*";

        bool result = sfd.ShowDialog(this) == DialogResult.OK;

        if (result)
        {
            last_exportcontininputfile = Path.GetFileNameWithoutExtension(sfd.FileName);
            ContinInput cinp = new ContinInput(av_correlations, lslider.Index, rslider.Index);
            ContinInterface100 cint = new ContinInterface100();
            StreamWriter SW = new StreamWriter(sfd.FileName, false, System.Text.Encoding.ASCII);
            cint.WriteContinInput(SW, cinp);
            SW.Close();
        }

        sfd.Dispose();
    }

    public void GetAllStats(bool allowcontintorun)
    {
        if (!getstatspending) return;
        else getstatspending = false;

        int len = -1;
        GetStats(anglestats, "Angle");
        GetStats(temperaturestats, "Temperature");
        GetStats(viscositystats, "Viscosity");
        GetStats(refractiveindexstats, "RefractiveIndex");

        if (listviewdatafiles.Items.Count > 0)
            len = ((DataFile)listviewdatafiles.Items[0]).CorSize;
        correlationstats.Deviation = "NaN";

        av_correlations = null;

        plotCorrelations.Graphs["average"].DSet = null;
        plotFit.Graphs["average"].DSet = null;
        plotFit.Graphs["fit"].DSet = null;

        if (listviewdatafiles.CheckedItems.Count > 0)
        {
            int count;
            av_correlations = new DataSet(len);
            av_correlations.X = ((DataFile)listviewdatafiles.CheckedItems[0]).Correlations[0].X;
            double[] av = av_correlations.Y;

            foreach (DataFile df in listviewdatafiles.CheckedItems)
            {
                count = df.Correlations.Count;
                foreach (DataSet ds in df.Correlations)
                {
                    for (int i = 0; i < len; i++) av[i] += ds.Y[i] / count;
                }
            }

            count = listviewdatafiles.CheckedItems.Count;
            for (int i = 0; i < len; i++) av[i] /= count;

            double deviation = 0;
            foreach (DataFile df in listviewdatafiles.CheckedItems)
            {
                count = df.Correlations.Count;
                double[] cor_df;

                if (count == 1) cor_df = df.Correlations[0].Y;
                else
                {
                    cor_df = new double[len];
                    foreach (DataSet ds in df.Correlations)
                    {
                        for (int i = 0; i < len; i++) cor_df[i] += ds.Y[i] / count;
                    }
                }

                for (int i = 0; i < len; i++) deviation += Math.Pow(cor_df[i] - av_correlations.Y[i], 2);
            }
            deviation /= len;
            correlationstats.Deviation = string.Format("{0,0:0.######}", listviewdatafiles.CheckedItems.Count > 1 ? Math.Sqrt(deviation / (listviewdatafiles.CheckedItems.Count - 1)) : 0);
        }

        plotCorrelations.Graphs["average"].DSet = av_correlations;
        plotFit.Graphs["average"].DSet = av_correlations;

        if (allowcontintorun && listviewdatafiles.CheckedItems.Count > 0) RunContinClick(null, EventArgs.Empty);
    }

    private void GetStats(StatData sd, string property)
    {
        double average = 0;
        double variance = 0;
        int count = listviewdatafiles.CheckedItems.Count;
        foreach (DataFile df in listviewdatafiles.CheckedItems)
        {
            average += (double)df.GetType().GetProperty(property).GetValue(df, null);
        }
        average /= count;

        if (count > 1)
        {
            foreach (DataFile df in listviewdatafiles.CheckedItems)
            {
                variance += Math.Pow((double)df.GetType().GetProperty(property).GetValue(df, null) - average, 2);
            }
            variance /= (count - 1);
        }

        sd.Value = average;
        sd.Average = string.Format("{0,0:0.#####}", average);
        sd.Deviation = string.Format("{0,0:0.######}", Math.Sqrt(variance));
        sd.N = count;
    }
}
