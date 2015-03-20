public class Preferences
{
	static double angletolerance = 0.05;
	
	static public double AngleTolerance {
		get {
			return angletolerance;
		}
		set {
			angletolerance = value;
		}
	}
}
