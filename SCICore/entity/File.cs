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
    /// If it is a directory item, then this field means subdirectories.
    /// Otherwise, this field is useless.
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
    public string FileName { get; set; }
    public ItemType Type { get; set; }
    public long FileSize { get; set; }
    public HashResult FileHashResult { get; set; }
    public string ArchiveName { get; set; }
    public long ArchiveSize { get; set; }
    public HashResult ArchiveHashResult { get; set; }
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
}