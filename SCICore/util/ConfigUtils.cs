using SCICore.entity;

namespace SCICore.util;

public static class ConfigUtils
{
    private const string ConfigFileName = "config.json";

    private const string EncDbFileName = "enc.db";

    private const string DecDbFileName = "dec.db";

    /// <summary>
    /// a list of directories that maybe contains config file, ordered by priority
    /// </summary>
    private static readonly string[] ConfigDirs =
    {
        AppContext.BaseDirectory,
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SecureCloudInspector")
    };


    /// <summary>
    /// return the path to the config file with the highest priority
    /// </summary>
    public static string? FindConfigPath()
    {
        foreach (var p in ConfigDirs)
        {
            var f = Path.Join(p, ConfigFileName);
            if (File.Exists(f))
            {
                return f;
            }
        }

        return null;
    }

    public static string GetDefaultConfigPath()
    {
        return Path.Join(ConfigDirs[^1], ConfigFileName);
    }


    /// <summary>
    /// create config file at default location if not existed
    /// </summary>
    public static async Task<bool> CreateDefaultConfig()
    {
        var configFile = GetDefaultConfigPath();
        if (File.Exists(configFile))
        {
            return false;
        }

        if (!Directory.Exists(ConfigDirs[^1]))
        {
            Directory.CreateDirectory(ConfigDirs[^1]);
        }

        var config = new Config
        {
            EncDbPath = Path.Join(ConfigDirs[^1], EncDbFileName),
            DecDbPath = Path.Join(ConfigDirs[^1], DecDbFileName)
        };

        await JsonUtils.Write(configFile, config, pretty: true);
        Console.WriteLine($"default config file created at {configFile}");
        return true;
    }
}