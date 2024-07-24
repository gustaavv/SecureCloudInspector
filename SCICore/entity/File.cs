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
            if (child.ArchiveName == null) continue;
            map[child.ArchiveName] = child;
        }

        return map;
    }
}