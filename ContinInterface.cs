using System;
using System.IO;
using System.Windows.Forms;
using Dullware.Plotter;

public class ContinInterfacex
{
    static object lockThis = new object();
    string errormessage;
    public string ErrorMessage
    {
        get { return errormessage; }
    }

    public string RunContin(ContinInput ci, ContinOutput co)
    {
        //WriteContinInputToFile(ci);
        lock (lockThis) //Er mag maar een contin tegelijk lopen, waarschijnlijk omdat ze allemaal dezelfde stdin en stdout gebruiken
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = Application.StartupPath + @"\Contin.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();
                WriteContinInput(proc.StandardInput, ci);
                ReadContinOutput(proc.StandardOutput, co);
                proc.WaitForExit();
                proc.Close();
                proc.Dispose();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        return null;
    }

    void WriteContinInputToFile(ContinInput ci)
    {
        StreamWriter SW = new StreamWriter("contin.inp", false, System.Text.Encoding.ASCII);
        WriteContinInput(SW, ci);
        SW.Close();
    }

    public bool WriteContinInput(StreamWriter SW, ContinInput ci)
    {
        int lowerindex = ci.Correlations.LowerBound;//Set in AngleTabPage.AddDataFile();
        int upperindex = ci.Correlations.UpperBound;//idem;
        int pksdelta = 6;

        //SW.WriteLine("{0,-23}: Theta={1,6:f2} Temp={2,6:f2} Visc={3,6:f4} n={4,8:f6} {5,5:f1}", "StandardInput", ang, temp - 273.15, visc, indx, wave);
        SW.WriteLine("AfterALV input file");

        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "LAST", 0, 1);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "GMNMX", 1, 1.0 / ci.Correlations.X[upperindex - pksdelta - 1] / 3.0);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "GMNMX", 2, 2.0 / ci.Correlations.X[lowerindex - 1]);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "IWT", 0, 5);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "NERFIT", 0, 0);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "NINTT", 0, -1);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "NLINF", 0, 0);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "MPKMOM", 0, 5);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "NG", 0, ci.GridSize);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:#.000000E+00}", "IQUAD", 0, 3);

        SW.WriteLine(" {0,-78}", "IFORMT");
        SW.WriteLine(" {0,-78}", "(5E14.6)");
        SW.WriteLine(" {0,-78}", "IFORMY");
        SW.WriteLine(" {0,-78}", "(5E14.6)");

        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "DOUSNQ", 0, 1);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "NONNEG", 0, 1);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "IUSER", 10, 2);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "RUSER", 16, 0);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "RUSER", 11, 10);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "RUSER", 10, -1);
        SW.WriteLine(" {0,-6}{1,5:D}{2,15:f6}", "IUNIT", 9, 9);

        SW.WriteLine(" {0,-6}", "END");
        SW.WriteLine(" {0,-6}{1,5:D}", "NY", upperindex - lowerindex + 1);

        int i = lowerindex;
        while (i <= upperindex)
        {
            for (int j = 0; j < 5 && i <= upperindex; j++) SW.Write("{0,14:#.000000E+00}", ci.Correlations.X[i++]);
            SW.WriteLine();
        }

        i = lowerindex;
        while (i <= upperindex)
        {
            for (int j = 0; j < 5 && i <= upperindex; j++) SW.Write("{0,14:#.000000E+00}", ci.Correlations.Y[i++]);
            SW.WriteLine();
        }

        return true;
    }

    private bool ReadContinOutput(StreamReader SR, ContinOutput co)
    {
        string s = null;

        try
        {
            ////Zoek eerst naar ng. Dit is het aantal rooster punten (grid points).
            //while ((s = SR.ReadLine()) != null && !s.StartsWith(" NG      ")) ;
            //ngrid = int.Parse(s.Substring(9));

            ////Zoek dat naar ny. Dit is het aantal data punten (input).
            //while ((s = SR.ReadLine()) != null && !s.StartsWith(" NY      ")) ;
            //ndata = int.Parse(s.Substring(9));

            //Zoek dan naar de tweede 1CONTIN
            for (int i = 0; i < 2; i++) while ((s = SR.ReadLine()) != null && !s.StartsWith("1CONTIN")) ;
            //Dan naar 0PLOT
            while ((s = SR.ReadLine()) != null && !s.StartsWith("0PLOT OF DATA")) ;
            //Dan naar ordinate  abscissa
            while ((s = SR.ReadLine()) != null && !s.StartsWith("    ORDINATE  ABSCISSA")) ;

            //Nu zijn we bij de fit data
            for (int i = 0; i < co.Correlations.Length; i++)
            {
                s = SR.ReadLine();
                //de X-waarden s.Substring(12, 10)) zijn onnauwkeuriger dan de waarden in de DAT file.
                // in de file staat g1, maak hier g1^2 van.
                co.Correlations.Y[i] = Math.Pow(double.Parse(s.Substring(1, 11)), 2);
            }

            //Zoek vervolgens naar de derde 1CONTIN
            while ((s = SR.ReadLine()) != null && !s.StartsWith("1CONTIN")) ;
            //Dan naar     ordinate    error  abscissa
            while ((s = SR.ReadLine()) != null && !s.StartsWith("    ORDINATE    ERROR  ABSCISSA")) ;

            //Nu zijn we bij de inverse-Laplace-getransformeerde
            for (int i = 0; i < co.Transform.Length; i++)
            {
                s = SR.ReadLine();
                co.Transform.X[i] = double.Parse(s.Substring(21, 10));
                co.Transform.Y[i] = double.Parse(s.Substring(1, 11));
            }

            co.Peaks.Clear();
            while ((s = SR.ReadLine()) != null)
            {
                if (s.StartsWith("0PEAK"))
                {
                    ContinPeak p = new ContinPeak();
                    co.Peaks.Add(p);
                    if ((s = SR.ReadLine()) == null) break; // -1ste moment
                    if ((s = SR.ReadLine()) == null) break; // 0de moment
                    p.Area = double.Parse(s.Substring(49, 7)) * Math.Pow(10, double.Parse(s.Substring(64, 4)));
                    if ((s = SR.ReadLine()) == null) break; // 1ste moment
                    p.Position = double.Parse(s.Substring(95, 11));
                }

                else if (s.StartsWith("                MOMENTS OF ENTIRE SOLUTION"))
                {
                    double area;
                    if ((s = SR.ReadLine()) == null) break; // -1ste moment
                    area = double.Parse(s.Substring(49, 7)) * Math.Pow(10, double.Parse(s.Substring(64, 4)));

                    for (int i = 0; i < co.Transform.Length; i++)
                        co.Transform.Y[i] /= area;
                }
            }
        }
        catch (Exception e)
        {
            errormessage = "Fatal error in contin output.\n\nCheck your boundaries.\n\n" + e.Message;
            return false;
        }

        //Multiply the transform by gamma
        Hocuspocus(co);

        return true;
    }

    void Hocuspocus(ContinOutput co)
    {
        double x1, x2, dx, xx;
        double y1, y2, dy, yh;

        for (int i = 1; i < co.Transform.Length; i++)
        {
            x2 = co.Transform.X[i];
            x1 = co.Transform.X[i - 1];
            y2 = co.Transform.Y[i];
            y1 = co.Transform.Y[i - 1];
            dx = x2 - x1;
            dy = y2 - y1;
            xx = x2 / x1;
            yh = (y2 + y1) / 2;
            co.Transform.X[i - 1] = (x2 + x1) / 2;
            co.Transform.Y[i - 1] = yh * dx / Math.Log10(xx);
        }

        // laatste punt wordt niet meer gebruikt
        co.Transform.UpperBound--;
    }
}
