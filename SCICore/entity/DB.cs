namespace SCICore.entity;

/// <summary>
/// Same password for every file in the encrypted folder.
/// Or, every file has its own password.
/// </summary>
public enum PasswordLevel
{
    File,
    Db
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

public record Database
{
    public string SourceFolder { get; set; }
    public string EncryptedFolder { get; set; }
    public Node Node { get; set; }
    public EncryptScheme EncryptScheme { get; set; }
    public string Password { get; set; }

    public Database()
    {
    }

    public Database(
        string sourceFolder,
        string encryptedFolder,
        Node node,
        EncryptScheme encryptScheme,
        string password
    )
    {
        SourceFolder = sourceFolder;
        EncryptedFolder = encryptedFolder;
        Node = node;
        EncryptScheme = encryptScheme;
        Password = password;
    }
}

public record DatabaseIndex
{
    /// <summary>
    /// key: path of a source folder. <br/>
    /// value: path of the db file.
    /// </summary>
    private Dictionary<string, string> SourceFolder2DbMap { get; set; }

    public DatabaseIndex()
    {
        SourceFolder2DbMap = new Dictionary<string, string>();
    }
}