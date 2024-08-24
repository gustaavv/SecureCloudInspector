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
    /// path to DB folder. Absolute or relative to config file's folder
    /// </summary>
    public string DbFolder { get; set; } = "./DB";

    public Config()
    {
    }
}