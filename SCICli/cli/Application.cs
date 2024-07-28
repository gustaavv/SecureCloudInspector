using System.Reflection;
using CommandLine;
using SCICli.dao;
using SCICli.entity;
using SCICli.util;
using SCICore.util;

namespace SCICli.cli;

public static class Application
{
    private static Type[] LoadVerbs()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
    }

    public static async Task Main(string[] args)
    {
        var types = LoadVerbs();
        var parserResult = Parser.Default.ParseArguments(args, types);
        await parserResult.WithParsedAsync(Run);
        await parserResult.WithNotParsedAsync(HandleError);
    }

    private static ConfigDao ConfigDao { get; set; }


    private static async Task Run(object obj)
    {
        ParseGlobalOptions((GlobalOptions)obj);

        if (!ConfigDao.Config.Init)
        {
            if (obj is ConfigOptions options)
            {
                if (!options.Init)
                {
                    Console.WriteLine("sci-cli is not initialized yet. Use 'config --init' first");
                    return;
                }
            }
            else
            {
                Console.WriteLine("sci-cli is not initialized yet. Use 'config --init' first");
                return;
            }
        }

        switch (obj)
        {
            case ConfigOptions opt:
                if (opt.Init)
                {
                }

                break;
            case PasswordOptions opt:
                break;
            case DatabaseOptions opt:
                var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());
                if (opt.List)
                {
                    var listDatabases = indexDao.ListDatabases();
                }

                break;
            case EncryptOptions opt:
                break;
            case DecryptOptions opt:
                break;
            default:
                Console.WriteLine("unknown option");
                break;
        }
    }

    private static void ParseGlobalOptions(GlobalOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConfigFile))
        {
            ConfigDao = new ConfigDao(options.ConfigFile);
        }
        else
        {
            var configFilePath = ConfigUtils.FindConfigPath()!;
            ConfigDao = new ConfigDao(configFilePath);
        }
    }

    private static async Task HandleError(IEnumerable<Error> errors)
    {
        foreach (var e in errors)
        {
            Console.WriteLine(e);
        }
    }
}