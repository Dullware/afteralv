using Dullware.Plotter;

public class ContinInput
{
    int gridsize = 100;
    public int GridSize
    {
        get { return gridsize; }
        set { gridsize = value; }
    }

    DataSet correlations;
    public DataSet Correlations
    {
        get { return correlations; }
        //set { correlations = value; }
    }

    public ContinInput(DataSet av_correlations, int lb, int ub)
    {
        correlations = new DataSet(av_correlations.X, av_correlations.Y);
        correlations.LowerBound = lb;
        correlations.UpperBound = ub;
    }
}