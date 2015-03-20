using System.Text;
using System.Runtime.InteropServices;

public class Win32API
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
    static extern bool PathRelativePathTo(StringBuilder pszPath, string pszFrom, [MarshalAs(UnmanagedType.U4)] int dwAttrFrom, string pszTo, [MarshalAs(UnmanagedType.U4)] int dwAttrTo);
    const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

    public static bool PathRelativePathTo(ref string relpath, string refpath, string path)
    {
        // Create the buffer for the string Builder. 
        // MAX_PATH = 260. 
        // This function ASSUMES that the buffer is initialized to at least MAX_PATH characters. 
        StringBuilder pobjPath = new StringBuilder(260);
        // Make the call. 
        bool result = PathRelativePathTo(pobjPath, refpath, 0, path, 0);
        relpath = pobjPath.ToString();
        return result;
    }


    //    The FILE_ATTRIBUTE_DIRECTORY value is used to specify that the path is a directory. 

}