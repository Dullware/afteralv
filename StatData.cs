using System;

public class StatData
{
    public event EventHandler AverageChanged;
    public event EventHandler DeviationChanged;
    public event EventHandler NChanged;

    double value;
    public double Value
    {
        get { return this.value; }
        set { this.value = value; }
    }

    string average;

    public string Average
    {
        get { return average; }
        set
        {
            average = value;
            if (AverageChanged != null) AverageChanged(this, new EventArgs());
        }
    }
    string deviation;

    public string Deviation
    {
        get { return deviation; }
        set
        {
            deviation = value;
            if (DeviationChanged != null) DeviationChanged(this, new EventArgs());
        }
    }
    int n;

    public int N
    {
        get { return n; }
        set
        {
            n = value;
            if (NChanged != null) NChanged(this, new EventArgs());
        }
    }
}
