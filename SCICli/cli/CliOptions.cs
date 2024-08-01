using CommandLine;

namespace SCICli.cli;

public abstract class GlobalOptions
{
    [Option("configFile", Default = null, HelpText = "to switch over different configs")]
    public string? ConfigFile { get; set; }
}

[Verb("config", HelpText = "")]
public class ConfigOptions : GlobalOptions
{
    [Option("init", Default = false, SetName = "init")]
    public bool Init { get; set; }
}

[Verb("db", HelpText = "")]
public class DatabaseOptions : GlobalOptions
{
    [Option("create", Default = false, SetName = "create")]
    public bool Create { get; set; }

    [Option("rename", Default = false, SetName = "rename")]
    public bool Rename { get; set; }

    [Option("delete", Default = false, SetName = "delete")]
    public bool Delete { get; set; }

    [Option("list", Default = false, SetName = "list")]
    public bool List { get; set; }
}

[Verb("enc", HelpText = "")]
public class EncryptOptions : GlobalOptions
{
}

[Verb("dec", HelpText = "")]
public class DecryptOptions : GlobalOptions
{
}

[Verb("util", HelpText = "")]
public class UtilOptions : GlobalOptions
{
    [Option("cmp_dir", Default = false, SetName = "cmp_dir")]
    public bool CompareDir { get; set; }

    [Option("cal_pwd", Default = false, SetName = "cal_pwd",
        HelpText = "calculate the password of an encrypted archive if users want to manually extract the archive")]
    public bool CalculatePwd { get; set; }
}