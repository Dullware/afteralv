using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Dullware.Plotter;
using System.Collections.Generic;

public class DataFile : ListViewItem
{
    private string filename;
    private string datetime, samplename;
    private double temperature, angle, viscosity, refractiveindex, wavelength, duration;
    Int32 corsize;

    public Int32 CorSize
    {
        get { return corsize; }
        set { corsize = value; }
    }

    public string FileName
    {
        get { return filename; }
    }
    private string mode;

    public string Mode
    {
        get
        {
            return mode;
        }
    }

    public string Datetime
    {
        get
        {
            return datetime;
        }
    }
    public string SampleName
    {
        get
        {
            return samplename;
        }
    }

    public double Temperature
    {
        get
        {
            return temperature;
        }
    }

    public double Angle
    {
        get { return angle; }
    }

    public double Viscosity
    {
        get { return viscosity; }
    }

    public double RefractiveIndex
    {
        get { return refractiveindex; }
    }

    public double WaveLength
    {
        get { return wavelength; }
    }

    private List<DataSet> correlations = new List<DataSet>();

    public List<DataSet> Correlations
    {
        get
        {
            return correlations;
        }
    }

    private List<DataSet> countrates = new List<DataSet>();

    public List<DataSet> CountRates
    {
        get
        {
            return countrates;
        }
    }

    string errormessage;
    public string ErrorMessage
    {
        get { return errormessage; }
    }

    //new public bool Checked
    //{
    //    get
    //    {
    //        return base.Checked;
    //    }
    //    set
    //    {
    //        base.Checked = value;
    //    }
    //}

    public DataFile(string filename)
    {
        this.filename = filename;
    }

    public bool ReadData()
    {
        StreamReader SR = null;
        try
        {
            SR = new StreamReader(filename, System.Text.Encoding.ASCII);
        }
        catch (Exception e)
        {
            if (SR != null) SR.Close();
            errormessage = e.Message;
            return false;
        }
        string s;

        s = SR.ReadLine();
        if (s.StartsWith("DASC") && s.Length > 7 && s[7] == 'S') //ALV DOS Data File
        {
            string tag;
            string data;
            int imode;
            int first = 0;
            int sca0;
            double averagetrace;
            double trst = 0;

            while (s != null)
            {
                if (s.Length > 3)
                {
                    tag = s.Substring(0, 4);
                    try
                    {

                        data = s.Substring(9);
                        if (tag == "DASC") mode = data;
                        else if (tag == "MODE") imode = Convert.ToInt32(data);
                        else if (tag == "DATI") datetime = data;
                        else if (tag == "SMPL") samplename = data;
                        else if (tag == "TEMP") temperature = Convert.ToDouble(data);
                        else if (tag == "ANG ") angle = Convert.ToDouble(data);
                        else if (tag == "VISC") viscosity = Convert.ToDouble(data);
                        else if (tag == "INDX") refractiveindex = Convert.ToDouble(data);
                        else if (tag == "WAVE") wavelength = Convert.ToDouble(data);
                        else if (tag == " DUR") duration = Convert.ToDouble(data);
                        else if (tag == " 1ST")
                        {
                            first = Convert.ToInt32(data);
                            if (first != 1)
                            {
                                SR.Close();
                                return false;
                            }
                        }
                        else if (tag == "COR0")
                        {
                            Int32 i = 0;
                            corsize = Convert.ToInt32(s.Substring(4, 3));
                            if (first == 0) corsize--; // Eerste datapunt is onzin.

                            correlations.Add(new DataSet(string.Format("correlations_{0}",Path.GetRandomFileName()),corsize));

                            while (i < corsize)
                            {
                                s = SR.ReadLine();
                                for (int j = 0; j < 4 && i < corsize; j++)
                                    if (!(first == 0 && i == 0 && j == 0)) // Sla het eerste onzinpunt over.
                                        correlations[0].Y[i++] = double.Parse(s.Substring(j * 15, 15)) - 1;
                            }
                        }
                        else if (tag == "TRST") trst = Convert.ToDouble(data);
                        else if (tag == "RAT0") averagetrace = Convert.ToDouble(data);
                        else if (tag == "SCA0") sca0 = Convert.ToInt32(data);
                        else if (tag == "TRA0")
                        {
                            Int32 i = 0, n = Convert.ToInt32(s.Substring(4, 3));
                            countrates.Add(new DataSet(string.Format("countrates_{0}",Path.GetRandomFileName()),n));

                            while (i < n)
                            {
                                s = SR.ReadLine();
                                for (int j = 0; j < 4 && i < n; j++)
                                {
                                    countrates[0].X[i] = (i + 1) * trst / 1000d;
                                    countrates[0].Y[i++] = double.Parse(s.Substring(j * 15, 15));
                                }
                            }
                        }
                    }
                    catch
                    {
                        errormessage = "Your data file format is not ALV compliant.\n\n\nOffending line:\n" + s;
                        SR.Close();
                        return false;
                    }
                }

                s = SR.ReadLine();
            }
            if (samplename == null) samplename = datetime;
            if (mode.IndexOf("FAST") == -1) CalcSingleLagData();
            else SetFastLagData();
        }
        else if (s.StartsWith("ALV-")) // ALV Windows Data File
        {
            int runs;

            while (s != null)
            {
                string[] arg = s.Split(new char[] { '\t' });

                if (s.StartsWith("Date :")) datetime = s.Split(new char[] { '\"' })[1];
                if (s.StartsWith("Time :")) datetime += " " + s.Split(new char[] { '\"' })[1];
                if (s.StartsWith("Samplename :")) samplename = s.Split(new char[] { '\"' })[1];

                if (s.StartsWith("Temperature [K] :")) temperature = double.Parse(arg[1]);
                if (s.StartsWith("Viscosity [cp]  :")) viscosity = double.Parse(arg[1]); // 1cp = 1mPI = 0.001 Pa s
                if (s.StartsWith("Refractive Index:")) refractiveindex = double.Parse(arg[1]);
                if (s.StartsWith("Wavelength [nm] :")) wavelength = double.Parse(arg[1]);
                //if (s.StartsWith("Angle [°]       :")) angle = double.Parse(arg[1]);
                if (s.StartsWith("Angle [?]       :")) angle = double.Parse(arg[1]);
                if (s.StartsWith("Duration [s]    :")) duration = double.Parse(arg[1]);

                if (s.StartsWith("Runs            :"))
                {
                    runs = int.Parse(arg[1]);
                    if (runs > 1)
                    {
                        errormessage = "Data files contaning multiple runs are not yet supported";
                        SR.Close();
                        return false;
                    }
                }
                if (s.StartsWith("Mode            :")) mode = s.Split(new char[] { '\"' })[1];

                if (s.StartsWith("\"Correlation\""))
                {
                    List<string> tmpcopy = new List<string>(); // Is to be filled with all data lines from the correlation section

                    s = SR.ReadLine();
                    while (s != null && s != "")
                    { // the correlation data section is read and put in tmpcopy
                        tmpcopy.Add(s);
                        s = SR.ReadLine();
                    }
                    corsize = tmpcopy.Count; // number of data points

                    if (corsize > 0)
                    {
                        int ncols;
                        // first find out ncols based on the first line tmpcopy[0]
                        arg = tmpcopy[0].Split(new char[] { '\t' });
                        // ncols is set to the number of non-zero columns
                        for (ncols = 1; ncols < arg.Length && double.Parse(arg[ncols]) != 0; ncols++);
                    
                        for (int j = 1; j < ncols; j++)
                            correlations.Add(new DataSet(string.Format("correlations{0}_{1}",j,Path.GetRandomFileName()),corsize));

                        for (int i = 0; i < corsize; i++)
                        {
                            arg = tmpcopy[i].Split(new char[] { '\t' });
                            for (int j = 1; j < ncols; j++)
                            {
                                correlations[j - 1].X[i] = double.Parse(arg[0]) / 1000; // ms -> s
                                correlations[j - 1].Y[i] = double.Parse(arg[j]);
                            }
                        }
                        tmpcopy.Clear();
                    }
                }

                if (s.StartsWith("\"Count Rate\""))
                {
                    List<string> tmpcopy = new List<string>();

                    s = SR.ReadLine();
                    while (s != null && s != "")
                    {
                        tmpcopy.Add(s);
                        s = SR.ReadLine();
                    }

                    if (tmpcopy.Count > 0)
                    {
                        int ncols;
                        // first find out ncols based on the first line tmpcopy[0]
                        arg = tmpcopy[0].Split(new char[] { '\t' });
                        // ncols is set to the number of non-zero columns
                        for (ncols = 1; ncols < arg.Length && double.Parse(arg[ncols]) != 0; ncols++);

                        for (int j = 1; j < ncols; j++)
                            countrates.Add(new DataSet(string.Format("countrates{0}_{1}",j,Path.GetRandomFileName()),tmpcopy.Count));

                        for (int i = 0; i < tmpcopy.Count; i++)
                        {
                            arg = tmpcopy[i].Split(new char[] { '\t' });
                            for (int j = 1; j < ncols; j++)
                            {
                                countrates[j - 1].X[i] = double.Parse(arg[0]) / 1000d;
                                countrates[j - 1].Y[i] = double.Parse(arg[j]);
                            }
                        }
                        tmpcopy.Clear();
                    }
                }

                s = SR.ReadLine();
            }
        }
        else
        {
            errormessage = "Your data file format is not ALV compliant.\n\n\nOffending line:\n" + s;
            SR.Close();
            return false;
        }
        SR.Close();

        Text = Path.GetFileNameWithoutExtension(FileName);
        ToolTipText = FileName;
        SubItems.Add(string.Format("{0}", correlations.Count));
        SubItems.Add(samplename);
        SubItems.Add(datetime);
        SubItems.Add(string.Format("{0}", angle));
        SubItems.Add(string.Format("{0}", temperature));
        SubItems.Add(string.Format("{0}", viscosity));
        SubItems.Add(string.Format("{0}", refractiveindex));

        return true;
    }

    private void CalcSingleLagData()
    {
        double[] tau = new double[correlations[0].Length];
        int i, n = tau.Length;
        double lag = 0.0002e-3; // s ipv ms
        double tau0;

        tau0 = 0;

        for (i = 0; i < 8 && i < n; i++) tau[i] = tau0 += lag;

        for (int j = 0; i < n; j++)
        {
            for (; i < 16 + 8 * j && i < n; i++) tau[i] = tau0 += lag;
            lag *= 2;
        }

        foreach (DataSet ds in correlations) ds.X = tau;
    }

    private void SetSingleLagData()
    {
        double[] tau = new double[correlations[0].Length];
        for (int i = 0; i < tau.Length; i++) tau[i] = LagData.alvsingle[i + 1] / 1000; // ms -> s.
        foreach (DataSet ds in correlations) ds.X = tau;
    }

    private void SetFastLagData()
    {
        double[] tau = new double[correlations[0].Length];
        for (int i = 0; i < tau.Length; i++) tau[i] = LagData.alvfast[i + 1] / 1000; // ms -> s.
        foreach (DataSet ds in correlations) ds.X = tau;
    }
}

