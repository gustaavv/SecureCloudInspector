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

[Verb("pwd", HelpText = "")]
public class PasswordOptions : GlobalOptions
{
}

[Verb("db", HelpText = "")]
public class DatabaseOptions : GlobalOptions
{
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