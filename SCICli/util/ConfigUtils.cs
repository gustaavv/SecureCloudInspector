﻿using SCICli.entity;
using SCICore.util;

namespace SCICli.util;

public static class ConfigUtils
{
    private const string ConfigFileName = "sci-config.json";

    /// <summary>
    /// a list of directories that maybe contains config file, ordered by priority
    /// </summary>
    private static readonly string[] ConfigDirs =
    {
        AppContext.BaseDirectory,
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SCI-CLI")
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
    /// create config file at default location
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

        await JsonUtils.Write(configFile, new Config(), pretty: true);
        Console.WriteLine($"default config file created at {configFile}");
        return true;
    }
}