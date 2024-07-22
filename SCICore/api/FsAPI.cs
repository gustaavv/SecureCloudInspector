using SCICore.entity;
using SCICore.util;

namespace SCICore.api;

/// <summary>
/// FileSystem API
/// </summary>
public static class FsApi
{
    public static Item BuildItemTree(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
        }

        var map = new Dictionary<string, Item>();

        var ans = new Item(FsUtils.GetLastEntry(path), ItemType.Dir);
        map[path] = ans;

        FsUtils.Walk(path, (root, dirs, files) =>
        {
            var parent = map[root];
            foreach (var d in dirs)
            {
                var item = new Item(FsUtils.GetLastEntry(d.ToString()), ItemType.Dir);
                parent.Children.Add(item);
                map[d.ToString()] = item;
            }

            foreach (var f in files)
            {
                var item = new Item(FsUtils.GetLastEntry(f.ToString()), ItemType.File)
                {
                    Size = f.Length
                };
                parent.Children.Add(item);
            }
        });

        return ans;
    }
}