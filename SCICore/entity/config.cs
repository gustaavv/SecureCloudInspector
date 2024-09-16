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

    public Config()
    {
    }
}