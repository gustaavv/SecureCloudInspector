using System.Reflection;
using CommandLine;
using ConsoleTables;
using SCICli.dao;
using SCICli.util;
using SCICore.api;
using SCICore.entity;
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
                    Console.WriteLine("SCI-CLI is not initialized yet. Use 'config --init' first");
                    return;
                }
            }
            else
            {
                Console.WriteLine("SCI-CLI is not initialized yet. Use 'config --init' first");
                return;
            }
        }

        switch (obj)
        {
            case ConfigOptions opt:
                if (opt.Init)
                {
                    if (ConfigDao.Config.Init)
                    {
                        Console.WriteLine("SCI-CLI has already been initialized.");
                        return;
                    }

                    var rarPath = InputUtils.Read("input rar path:");
                    var process = ProcessUtils.CreateProcess(rarPath, "");
                    var (success, output, error, exception) = ProcessUtils.RunProcess(process).Result;
                    if (success)
                    {
                        ConfigDao.Config.RarPath = rarPath;
                        ConfigDao.Config.Init = true;
                        _ = ConfigDao.WriteConfig();
                        Console.WriteLine("init succeed");
                    }
                    else
                    {
                        Console.WriteLine($"{rarPath} not a valid path");
                    }
                }

                break;

            case DatabaseOptions opt:
                var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());
                if (opt.Create)
                {
                    var db = new Database();
                    var dbName = InputUtils.Read("db name: ");
                    if (string.IsNullOrWhiteSpace(dbName))
                    {
                        Console.WriteLine("empty string is not valid");
                        return;
                    }

                    if (indexDao.GetIndex().ContainsKey(dbName))
                    {
                        Console.WriteLine("a db with the same name has already existed");
                        return;
                    }

                    if (DatabaseIndexDao.DbIndexFileName == $"{dbName}.json")
                    {
                        Console.WriteLine("this name is not allowed");
                        return;
                    }

                    var sourceFolder = InputUtils.Read("source folder: ");
                    sourceFolder = Path.GetFullPath(sourceFolder);
                    db.SourceFolder = sourceFolder;

                    if (!Directory.Exists(sourceFolder))
                    {
                        Console.WriteLine("this source folder doesn't exist");
                        return;
                    }

                    var encryptedFolder = InputUtils.Read("encrypted folder: ");
                    encryptedFolder = Path.GetFullPath(encryptedFolder);

                    if (encryptedFolder.Contains(sourceFolder))
                    {
                        Console.WriteLine("source folder should not be a child folder of encrypted folder");
                        return;
                    }

                    if (encryptedFolder.Contains(sourceFolder))
                    {
                        Console.WriteLine("encrypted folder should not be a child folder of source folder");
                        return;
                    }

                    db.EncryptedFolder = encryptedFolder;

                    var pwd = InputUtils.Read("user password:");

                    if (string.IsNullOrWhiteSpace(pwd))
                    {
                        Console.WriteLine("empty string is not valid");
                        return;
                    }

                    db.Password = pwd;
                    var encryptScheme = new EncryptScheme();
                    db.EncryptScheme = encryptScheme;

                    var num = InputUtils.Read("password level? (1) per file (2) whole directory:");
                    if (int.TryParse(num, out var pwdLevel))
                    {
                        if (pwdLevel is not (1 or 2))
                        {
                            Console.WriteLine("input 1 or 2 to choose the password level");
                            return;
                        }

                        encryptScheme.PwdLevel = pwdLevel == 1 ? PasswordLevel.File : PasswordLevel.Db;
                    }
                    else
                    {
                        Console.WriteLine("input 1 or 2 to choose the password level");
                        return;
                    }

                    encryptScheme.FileNamePattern = EncryptApi.MakePattern(15);
                    encryptScheme.PwdPattern = EncryptApi.MakePattern(60);

                    Console.WriteLine("confirm your input:");
                    Console.WriteLine($"source folder: {db.SourceFolder}");
                    Console.WriteLine($"encrypted folder: {db.EncryptedFolder}");
                    Console.WriteLine($"password: {db.Password}");
                    Console.WriteLine($"password level: {db.EncryptScheme.PwdLevel}");

                    var choice = InputUtils.Read("[y/n]?");
                    if (choice == "y")
                    {
                        var dbPath = Path.Join(indexDao.DbFolder, $"{dbName}.json");
                        await JsonUtils.Write(dbPath, db);
                        _ = new DbDao(dbPath); // validate db file creation
                        indexDao.GetIndex()[dbName] = dbPath;
                        _ = indexDao.WriteDbIndex();
                        Console.WriteLine("create success");
                    }
                    else
                    {
                        Console.WriteLine("not created");
                    }
                }
                else if (opt.Rename)
                {
                    var oldDbName = InputUtils.Read("db name: ");
                    if (!indexDao.GetIndex().ContainsKey(oldDbName))
                    {
                        Console.WriteLine("this db does not exist");
                        return;
                    }

                    var newDbName = InputUtils.Read("new db name: ");
                    if (string.IsNullOrWhiteSpace(newDbName))
                    {
                        Console.WriteLine("empty string is not valid");
                        return;
                    }

                    if (indexDao.GetIndex().ContainsKey(newDbName))
                    {
                        Console.WriteLine("this name already existed");
                        return;
                    }

                    if (DatabaseIndexDao.DbIndexFileName == $"{newDbName}.json")
                    {
                        Console.WriteLine("this name is not allowed");
                        return;
                    }

                    if (oldDbName == newDbName)
                    {
                        Console.WriteLine("no need to rename");
                        return;
                    }

                    var oldDbFile = indexDao.GetIndex()[oldDbName];
                    var newDbFile = Path.Join(Path.GetDirectoryName(oldDbFile), $"{newDbName}.json");

                    File.Move(oldDbFile, newDbFile);
                    _ = new DbDao(newDbFile); // validate db file move

                    indexDao.GetIndex().Remove(oldDbName);
                    indexDao.GetIndex()[newDbName] = newDbFile;
                    _ = indexDao.WriteDbIndex();
                    
                    Console.WriteLine($"rename succeed: {oldDbName} -> {newDbName}");
                }
                else if (opt.Delete)
                {
                }
                else if (opt.List)
                {
                    var nameDbTuples = await indexDao.ListDatabases();

                    var consoleTable = new ConsoleTable(new ConsoleTableOptions
                    {
                        Columns = new[] { "name", "source folder" },
                        EnableCount = true
                    });

                    foreach (var (name, db) in nameDbTuples)
                    {
                        consoleTable.AddRow(name, db.SourceFolder);
                    }

                    consoleTable.Write();
                }

                break;
            case PasswordOptions opt:
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
            if (configFilePath != null!)
            {
                ConfigDao = new ConfigDao(configFilePath);
            }
            else
            {
                var result = ConfigUtils.CreateDefaultConfig().Result;
                if (!result)
                {
                    throw new Exception("no default config file. Creation failed as well.");
                }
                else
                {
                    ConfigDao = new ConfigDao(ConfigUtils.GetDefaultConfigPath());
                }
            }
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