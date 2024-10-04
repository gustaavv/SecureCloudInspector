using ByteSizeLib;

namespace SCICore.util;

/**
 * FileSystem Utils
 */
public static class FsUtils
{
    public static string GetLastEntry(string path)
    {
        var ans = Path.GetFileName(path);

        // if the path to a dir ends with / or \
        if (string.IsNullOrEmpty(ans))
        {
            ans = new DirectoryInfo(path).Name;
        }

        return ans;
    }


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

    public static List<string> GetChildren(string dir, bool absolutePath = true)
    {
        var files = Directory.GetFiles(dir).Select(s => absolutePath ? s : GetLastEntry(s)).ToList();
        var directories = Directory.GetDirectories(dir).Select(s => absolutePath ? s : GetLastEntry(s)).ToList();
        files.AddRange(directories);
        return files;
    }

    public static string PrettyPrintBytes(long bytes)
    {
        var b = ByteSize.FromBytes(bytes);
        return b.LargestWholeNumberBinaryValue.ToString("0.00") + " " + b.LargestWholeNumberBinarySymbol;
    }
}