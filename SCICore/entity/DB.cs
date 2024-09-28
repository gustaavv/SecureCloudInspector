namespace SCICore.entity;

/// <summary>
/// Same password for every file in the encrypted folder.
/// Or, every file has its own password.
/// </summary>
public enum PasswordLevel
{
    File = 0,
    Db = 1
}

public record Pattern
{
    public int Md5 { get; set; }
    public int Sha1 { get; set; }
    public int Sha256 { get; set; }

    /// <summary>
    /// use prefix or suffix of the hash result
    /// </summary>
    public bool Prefix { get; set; }

    public Pattern()
    {
    }

    public Pattern(int md5, int sha1, int sha256, bool prefix)
    {
        Md5 = md5;
        Sha1 = sha1;
        Sha256 = sha256;
        Prefix = prefix;
    }
}

public record EncryptScheme
{
    public PasswordLevel PwdLevel { get; set; }
    public Pattern PwdPattern { get; set; }
    public Pattern FileNamePattern { get; set; }

    public EncryptScheme()
    {
    }

    public EncryptScheme(PasswordLevel pwdLevel) : this(
        pwdLevel,
        new Pattern(),
        new Pattern()
    )
    {
    }

    public EncryptScheme(PasswordLevel pwdLevel, Pattern pwdPattern, Pattern fileNamePattern)
    {
        PwdLevel = pwdLevel;
        PwdPattern = pwdPattern;
        FileNamePattern = fileNamePattern;
    }
}

public enum DbType
{
    Normal = 0
}

public record Database
{
    public string Name { get; set; }

    public string SourceFolder { get; set; }

    public string EncryptedFolder { get; set; }

    public Node Node { get; set; }

    public EncryptScheme EncryptScheme { get; set; }

    public string Password { get; set; }

    public DbType DbType { get; set; }

    /// <summary>
    /// Default is -1, means per file;
    /// 0 means the whole source folder;
    /// n means per n-level children;
    /// </summary>
    public int EncDepth { get; set; } = -1;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Database()
    {
    }

    public Database(
        string name,
        string sourceFolder,
        string encryptedFolder,
        Node node,
        EncryptScheme encryptScheme,
        string password,
        DbType dbType,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        Name = name;
        SourceFolder = sourceFolder;
        EncryptedFolder = encryptedFolder;
        Node = node;
        EncryptScheme = encryptScheme;
        Password = password;
        DbType = dbType;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}