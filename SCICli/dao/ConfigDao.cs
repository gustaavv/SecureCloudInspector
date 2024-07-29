using SCICli.entity;
using SCICore.util;

namespace SCICli.dao;

public class ConfigDao
{
    private string ConfigFilePath { get; set; }

    public Config Config { get; set; }

    public ConfigDao(string configFilePath)
    {
        ConfigFilePath = configFilePath;
        if (!File.Exists(ConfigFilePath))
        {
            throw new ArgumentException("not a valid file", nameof(configFilePath));
        }

        Config = JsonUtils.Read<Config>(ConfigFilePath).Result!;
    }

    public async Task WriteConfig()
    {
        await JsonUtils.Write(ConfigFilePath, Config, force: true, pretty: true);
    }

    public string GetDbFolder()
    {
        return Path.IsPathFullyQualified(Config.DbFolder)
            ? Config.DbFolder
            : Path.GetFullPath(Path.Join(Path.GetDirectoryName(ConfigFilePath), Config.DbFolder));
    }
}