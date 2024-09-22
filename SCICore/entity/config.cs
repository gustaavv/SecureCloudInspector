namespace SCICore.entity;

public record Config
{
    /// <summary>
    /// Whether the program has initialized
    /// </summary>
    public bool Init { get; set; } = false;

    /// <summary>
    /// Path to rar.exe
    /// </summary>
    public string? RarPath { get; set; }

    /// <summary>
    /// Path to Encrypt Db File
    /// </summary>
    public string EncDbPath { get; set; }

    /// <summary>
    /// Path to Decrypt Db File
    /// </summary>
    public string DecDbPath { get; set; }

    /// <summary>
    /// Create digest about databases when exporting
    /// </summary>
    public bool CreateDigestWhenExport { get; set; } = true;

    /// <summary>
    /// Verifying digest when importing databases
    /// </summary>
    public bool VerifyDigestWhenImport { get; set; } = true;

    /// <summary>
    /// Length of the encrypted filename/dirname. This property is used only
    /// when a database is created.
    /// </summary>
    public uint EncryptedFilenameLength { get; set; } = 15;

    /// <summary>
    /// Length of the actual password for the encrypted archive. This property
    /// is used only when a database is created.
    /// </summary>
    public uint EncryptedArchivePwdLength { get; set; } = 60;

    /// <summary>
    /// If set to 0, then encryption is performed on file level.
    /// <br/>
    /// If set to 1, then for every folder with height 1 (i.e. a folder with
    /// only files, no subfolders), it will be encrypted to an archive.
    /// <br/>
    /// In general, if set to N, then for every folder with height N, it will
    /// be encrypted to an archive. If the height of the entire folder tree is
    /// less than N, it will be encrypted to an archive. 
    /// </summary>
    public uint EncryptedTreeHeight { get; set; } = 0;

    /// <summary>
    /// Values <= 0 means do not check updates
    /// </summary>
    public int CheckUpdateFrequencyInHours { get; set; } = 24;

    /// <summary>
    /// 
    /// </summary>
    public int BackupFrequencyInDays { get; set; } = 7;

    /// <summary>
    /// 
    /// </summary>
    public string BackupFolderPath { get; set; }

    public Config()
    {
    }
}