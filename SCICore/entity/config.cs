namespace SCICore.entity;

public record Config
{
    /// <summary>
    /// Whether the program has initialized
    /// </summary>
    public bool Init { get; set; } = false;

    /// <summary>
    /// Path to WinRAR
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

    public uint EncryptedFilenameLength { get; set; } = 15;

    public uint EncryptedArchivePwdLength { get; set; } = 60;

    public Config()
    {
    }
}