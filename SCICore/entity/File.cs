namespace SCICore.entity;

public enum ItemType
{
    File,
    Dir
}

/// <summary>
/// Represents a directory item on the file system.
/// </summary>
public record Item
{
    /// <summary>
    /// Name of the item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Directory item can have different types.
    /// </summary>
    public ItemType Type { get; set; }

    /// <summary>
    /// Subdirectories. Useless if it is a file item.
    /// </summary>
    public List<Item> Children { get; set; }

    /// <summary>
    /// Size of a file. Useless if it is a directory item.
    /// </summary>
    public long Size { get; set; }

    public Item(string name, ItemType type, List<Item> children, long size = 0L)
    {
        Name = name;
        Type = type;
        Children = children;
        Size = size;
    }

    public Item(string name, ItemType type) :
        this(name, type, new List<Item>())
    {
    }

    public Item() : this(null!, ItemType.File, new List<Item>())
    {
    }
}

/// <summary>
/// Represents a record in the DB.
/// </summary>
public record Node
{
    /// <summary>
    /// Name of the original file/directory.
    /// </summary>
    public string FileName { get; set; }


    /// <summary>
    /// Directory item can have different types.
    /// </summary>
    public ItemType Type { get; set; }

    /// <summary>
    /// Size of the original file. Useless if it is a directory item.
    /// </summary>
    public long FileSize { get; set; }


    /// <summary>
    /// HashResult of the original file. Useless if it is a directory item.
    /// </summary>
    public HashResult FileHashResult { get; set; }

    /// <summary>
    /// For file, this is the archive name. For dir, this is the hashed name.
    /// </summary>
    public string ArchiveName { get; set; }

    /// <summary>
    /// Size of the archive. Useless if it is a directory item.
    /// </summary>
    public long ArchiveSize { get; set; }

    /// <summary>
    /// HashResult of the archive. Useless if it is a directory item.
    /// </summary>
    public HashResult ArchiveHashResult { get; set; }

    /// <summary>
    /// Subdirectories. Useless if it is a file item.
    /// </summary>
    public List<Node> Children { get; set; }

    public Node()
    {
    }

    public Node(string fileName, ItemType type) :
        this(
            fileName, type, 0L, null!,
            null!, 0L, null!,
            new List<Node>()
        )
    {
    }

    public Node(string fileName, ItemType type, long fileSize, HashResult fileHashResult) :
        this(
            fileName, type, fileSize, fileHashResult,
            null!, 0L, null!,
            new List<Node>()
        )
    {
    }

    public Node(
        string fileName, ItemType type, long fileSize, HashResult fileHashResult,
        string archiveName, long archiveSize, HashResult archiveHashResult,
        List<Node> children
    )
    {
        FileName = fileName;
        Type = type;
        FileSize = fileSize;
        FileHashResult = fileHashResult;
        ArchiveName = archiveName;
        ArchiveSize = archiveSize;
        ArchiveHashResult = archiveHashResult;
        Children = children;
    }

    /// <summary>
    /// key: child.FileName <br/>
    /// value: child node
    /// </summary>
    public Dictionary<string, Node> GetChildFileNameMap()
    {
        var map = new Dictionary<string, Node>();

        foreach (var child in Children)
        {
            map[child.FileName] = child;
        }

        return map;
    }


    /// <summary>
    /// key: child.ArchiveName (not null) <br/>
    /// value: child node.
    ///
    /// </summary>
    public Dictionary<string, Node> GetChildArchiveNameMap()
    {
        var map = new Dictionary<string, Node>();

        foreach (var child in Children)
        {
            if (child.ArchiveName == null!) continue;
            map[child.ArchiveName] = child;
        }

        return map;
    }

    /// <summary>
    /// get a child node in the tree rooted by this node.
    /// </summary>
    /// <param name="path">
    /// the path representing the child node. This path should begin with the root's name. <br/>
    /// E.g. path = "/a/b" means the root's name is "a" and the method will return its child "b". 
    /// path "a/b" is also ok. Do remember this is not a filepath, items like "." and ".." will be
    /// treated as files.
    /// </param>
    /// <param name="byFile">If set to true, path consists of file names.
    /// If set to false, path consists of archive names.</param>
    /// <returns> the child node if found. Else, null will be returned.</returns>
    public Node? GetChildBypath(string path, bool byFile)
    {
        var list = new List<string>() { byFile ? FileName : ArchiveName };
        path.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)).ToList().ForEach(s => list.Add(s));
        
        return GetChildBypath(this, list.ToArray(), 0, byFile);
    }

    private static Node? GetChildBypath(Node root, string[] path, int cur, bool byFile)
    {
        if (cur >= path.Length)
        {
            return null;
        }

        if (path[cur] != (byFile ? root.FileName : root.ArchiveName))
        {
            return null;
        }

        if (cur == path.Length - 1)
        {
            return root;
        }

        foreach (var child in root.Children)
        {
            if ((byFile ? child.FileName : child.ArchiveName) == path[cur + 1])
            {
                return GetChildBypath(child, path, cur + 1, byFile);
            }
        }

        return null;
    }
}