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

    private static ConfigDao ConfigDao { get; set; } = null!;

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
                    await ConfigInit();
                }

                break;
            case DatabaseOptions opt:
                if (opt.Create)
                {
                    await DatabaseCreate();
                }
                else if (opt.Rename)
                {
                    await DatabaseRename();
                }
                else if (opt.Delete)
                {
                    await DatabaseDelete();
                }
                else if (opt.List)
                {
                    await DatabaseList();
                }
                else if (opt.Search)
                {
                    await DatabaseSearch();
                }

                break;
            case EncryptOptions:
                await Encrypt();
                break;
            case DecryptOptions:
                await Decrypt();
                break;
            case UtilOptions opt:
                if (opt.CompareDir)
                {
                    await UtilCompareDir();
                }
                else if (opt.CalculatePwd)
                {
                    await UtilCalculatePwd();
                }

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

    private static async Task ConfigInit()
    {
        if (ConfigDao.Config.Init)
        {
            Console.WriteLine("SCI-CLI has already been initialized.");
            return;
        }

        var rarPath = InputUtils.Read("input rar path:");
        var process = ProcessUtils.CreateProcess(rarPath, "");
        var (success, _, _, _) = ProcessUtils.RunProcess(process).Result;
        if (success)
        {
            ConfigDao.Config.RarPath = rarPath;
            ConfigDao.Config.Init = true;
            await ConfigDao.WriteConfig();
            Console.WriteLine("init succeed");
        }
        else
        {
            Console.WriteLine($"{rarPath} not a valid path");
        }
    }

    private static async Task DatabaseCreate()
    {
        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

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
        if (choice.ToLower() == "y")
        {
            var dbPath = Path.Join(indexDao.DbFolder, $"{dbName}.json");
            await JsonUtils.Write(dbPath, db);
            _ = new DbDao(dbPath); // validate db file creation
            indexDao.GetIndex()[dbName] = dbPath;
            await indexDao.WriteDbIndex();
            Console.WriteLine("create success");
        }
        else
        {
            Console.WriteLine("not created");
        }
    }

    private static async Task DatabaseRename()
    {
        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

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
        await indexDao.WriteDbIndex();

        Console.WriteLine($"rename succeed: {oldDbName} -> {newDbName}");
    }

    private static async Task DatabaseDelete()
    {
        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

        var dbName = InputUtils.Read("db name: ");
        if (!indexDao.GetIndex().ContainsKey(dbName))
        {
            Console.WriteLine("this db does not exist");
            return;
        }

        var dbDao = new DbDao(indexDao.GetIndex()[dbName]);

        Console.WriteLine($"source folder of this db: {dbDao.Db.SourceFolder}");
        var choice = InputUtils.Read("Confirm delete? [y/n]:");

        if (choice.ToLower() == "y")
        {
            indexDao.GetIndex().Remove(dbName);
            await indexDao.WriteDbIndex();
            File.Delete(dbDao.DbFilePath);
            Console.WriteLine(
                $"delete succeed. Feel free to delete the encrypted folder: {dbDao.Db.EncryptedFolder}");
        }
        else
        {
            Console.WriteLine("not deleted");
        }
    }

    private static async Task DatabaseList()
    {
        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

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

    private static async Task DatabaseSearch()
    {
        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

        var dbName = InputUtils.Read("db name: ");
        if (!indexDao.GetIndex().ContainsKey(dbName))
        {
            Console.WriteLine("this db does not exist");
            return;
        }

        var dbDao = new DbDao(indexDao.GetIndex()[dbName]);

        var input = InputUtils.Read("search keywords (if multiple, separate them with spaces): ");
        var keywords = input.Split(' ')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.ToLower())
            .ToArray();

        var results = dbDao.Db.Node.Search(keywords);
        var consoleTable = new ConsoleTable(new ConsoleTableOptions
            {
                Columns = new[] { "path in source folder", "path in encrypted folder" },
                EnableCount = true
            }
        );

        foreach (var (_, srcPath, encPath) in results)
        {
            consoleTable.AddRow(srcPath, encPath);
        }

        consoleTable.Write();
    }

    private static async Task Encrypt()
    {
        ArchiveUtils.RarPath = ConfigDao.Config.RarPath;

        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

        var dbName = InputUtils.Read("db name: ");
        if (!indexDao.GetIndex().ContainsKey(dbName))
        {
            Console.WriteLine("this db does not exist");
            return;
        }

        var dbDao = new DbDao(indexDao.GetIndex()[dbName]);

        var sourceFolder = dbDao.Db.SourceFolder;
        if (!Directory.Exists(sourceFolder))
        {
            Console.WriteLine($"source folder does not exist: {sourceFolder}. Consider create a new db.");
            return;
        }

        var encryptedFolder = dbDao.Db.EncryptedFolder;
        if (!Directory.Exists(encryptedFolder))
        {
            Console.WriteLine($"create encrypted folder: {encryptedFolder}");
            Directory.CreateDirectory(encryptedFolder);
        }

        var choice = InputUtils.Read("Confirm encrypt? [y/n]:");
        if (choice.ToLower() == "y")
        {
            Console.WriteLine("please wait patiently...");
            await SciApi.EncryptData(dbDao.Db);
            await dbDao.WriteDb();
            Console.WriteLine("encrypt success");
        }
        else
        {
            Console.WriteLine("not encrypt");
        }
    }

    private static async Task Decrypt()
    {
        ArchiveUtils.RarPath = ConfigDao.Config.RarPath;

        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

        var dbName = InputUtils.Read("db name: ");
        if (!indexDao.GetIndex().ContainsKey(dbName))
        {
            Console.WriteLine("this db does not exist");
            return;
        }

        var dbDao = new DbDao(indexDao.GetIndex()[dbName]);

        var encFolder = InputUtils.Read("encrypted folder path:");
        if (!Directory.Exists(encFolder))
        {
            Console.WriteLine("this encrypted folder does not exist");
            return;
        }

        var decFolder = InputUtils.Read("decrypted folder path:");
        if (!Directory.Exists(decFolder))
        {
            Directory.CreateDirectory(decFolder);
            Console.WriteLine($"create decrypted folder: {decFolder}");
        }
        else if (new DirectoryInfo(decFolder).GetFiles().Length > 0 ||
                 new DirectoryInfo(decFolder).GetDirectories().Length > 0)
        {
            Console.WriteLine("There are items in this decrypted folder, choose another one.");
            return;
        }


        var choice = InputUtils.Read("Confirm decrypt? [y/n]:");
        if (choice.ToLower() == "y")
        {
            Console.WriteLine("please wait patiently...");
            await SciApi.DecryptData(encFolder, decFolder, dbDao.Db);
            Console.WriteLine("decrypt success");
        }
        else
        {
            Console.WriteLine("not decrypt");
        }
    }

    private static async Task UtilCompareDir()
    {
        var f1 = InputUtils.Read("folder 1:");
        var f2 = InputUtils.Read("folder 2:");

        var result = await FsApi.CompareDir(f1, f2);
        var msg = result ? "the same" : "different";
        Console.WriteLine($"The two folders are {msg}.");
    }

    private static async Task UtilCalculatePwd()
    {
        var indexDao = new DatabaseIndexDao(ConfigDao.GetDbFolder());

        var dbName = InputUtils.Read("db name: ");
        if (!indexDao.GetIndex().ContainsKey(dbName))
        {
            Console.WriteLine("this db does not exist");
            return;
        }

        var dbDao = new DbDao(indexDao.GetIndex()[dbName]);

        var pwdLevel = dbDao.Db.EncryptScheme.PwdLevel;
        Console.WriteLine($"password level: {pwdLevel}");

        if (pwdLevel == PasswordLevel.Db)
        {
            var pwd = await EncryptApi.MakeArchivePwd(dbDao.Db, null);
            Console.WriteLine($"password for {dbName} is: {pwd}");
        }
        else if (pwdLevel == PasswordLevel.File)
        {
            var filepath = InputUtils.Read("which archive? input the absolute path: ");
            filepath = Path.GetFullPath(filepath);

            if (!File.Exists(filepath))
            {
                Console.WriteLine("this archive does not exist");
                return;
            }

            if (!filepath.Contains(dbDao.Db.EncryptedFolder))
            {
                Console.WriteLine("this archive does not belong to this db.");
                return;
            }

            var virtualPath = filepath.Substring(dbDao.Db.EncryptedFolder.Length);
            virtualPath = virtualPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var root = dbDao.Db.Node;
            var child = root.GetChildBypath(virtualPath, false);
            if (child == null)
            {
                Console.WriteLine("such file is not found in the db. run `enc` first to track this file");
                return;
            }

            var pwd = await EncryptApi.MakeArchivePwd(dbDao.Db, child.FileName);
            Console.WriteLine($"password for {child.ArchiveName} is: {pwd}");
        }
        else
        {
            Console.WriteLine($"unknown password level: {pwdLevel}");
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