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

    /// <summary>
    /// compare two directories:
    /// (1) folder structures should be the same;
    /// (2) files should be the same.
    /// </summary>
    /// <returns> True if the folder structures are the same</returns>
    public static async Task<bool> CompareDir(string dir1, string dir2)
    {
        var b1 = Directory.Exists(dir1);
        var b2 = Directory.Exists(dir2);
        if (b1 ^ b2) return false; // one exists while the other don't
        if (!b1) return true; // both don't exist
        var t1 = BuildItemTree(dir1);
        var t2 = BuildItemTree(dir2);

        return await CompareDir(t1, t2, dir1, dir2);
    }

    private static async Task<bool> CompareDir(Item item1, Item item2, string path1, string path2)
    {
        if (item1.Type != ItemType.Dir || item2.Type != ItemType.Dir)
        {
            throw new Exception("item is not Dir type");
        }

        if (item1.Children.Count != item2.Children.Count)
        {
            return false;
        }

        var tasks = new List<Task<bool>>();

        var map = item1.Children.ToDictionary(i => i.Name);

        foreach (var child2 in item2.Children)
        {
            if (!map.ContainsKey(child2.Name))
            {
                return false;
            }

            var child1 = map[child2.Name];

            if (child1.Type != child2.Type)
            {
                return false;
            }

            var childPath1 = Path.Join(path1, child1.Name);
            var childPath2 = Path.Join(path2, child2.Name);

            if (child1.Type == ItemType.File)
            {
                tasks.Add(Task.Run(() =>
                    {
                        var hr1 = HashUtils.ComputeFileHash(childPath1).Result;
                        var hr2 = HashUtils.ComputeFileHash(childPath2).Result;
                        return hr1 == hr2;
                    })
                );
            }
            else if (child1.Type == ItemType.Dir)
            {
                tasks.Add(CompareDir(child1, child2, childPath1, childPath2));
            }
            else
            {
                return false;
            }
        }

        // true if all sub calls return true <=> false if any sub call return false
        return (await Task.WhenAll(tasks)).ToList().Any(b => !b);
    }
}