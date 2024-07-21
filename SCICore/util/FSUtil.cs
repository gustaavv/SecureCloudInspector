namespace SCICore.util;

/**
 * FileSystem Utils
 */
public static class FsUtil
{
    /// <summary>  Walk through the directory like python's os.walk(). <br/>
    /// This method should not be async because preorder traversal matters. 
    /// </summary>
    /// <param name="dir">path to the directory.</param>
    /// <param name="func"> func serves as a visitor function, it takes three parameters:
    /// root, dirs and files.</param>
    public static void Walk(string dir, Action<string, DirectoryInfo[], FileInfo[]> func)
    {
        var root = new DirectoryInfo(dir);
        var directories = root.GetDirectories();
        var files = root.GetFiles();

        // Invoke the Action delegate
        func(root.ToString(), directories, files);

        // Recursively walk through subdirectories
        foreach (var d in directories)
        {
            Walk(d.FullName, func);
        }
    }
}