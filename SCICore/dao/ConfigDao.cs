using SCICore.entity;
using SCICore.util;

namespace SCICore.dao;

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
}