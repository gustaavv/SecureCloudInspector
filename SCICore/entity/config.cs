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

    /// <summary>
    /// Only used for DecryptWindow of SCI-Desktop
    /// </summary>
    public string PreferredDecryptedPath { get; set; }

    public Config()
    {
    }
}