using System.Collections.Generic;
using Dullware.Plotter;

public class ContinOutput
{
    int datasize;
    public int DataSize
    {
        get { return datasize; }
        //set { datasize = value; }
    }

    int gridsize;
    public int GridSize
    {
        get { return gridsize; }
        //set { gridsize = value; }
    }

    DataSet correlations; //De gefitte waarden
    public DataSet Correlations
    {
        get { return correlations; }
        //set { correlations = value; }
    }

    DataSet transform; //Inverse laplace van contin
    public DataSet Transform
    {
        get { return transform; }
        //set { transform = value; }
    }

    List<ContinPeak> peak = new List<ContinPeak>();
    public List<ContinPeak> Peaks
    {
        get { return peak; }
    }

    public ContinOutput(int datasize, int gridsize)
    {
        this.datasize = datasize;
        this.gridsize = gridsize;

        correlations = new DataSet(datasize);
        transform = new DataSet(gridsize);
    }
}