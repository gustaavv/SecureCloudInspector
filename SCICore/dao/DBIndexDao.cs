using SCICore.entity;
using SCICore.util;

namespace SCICore.dao;

public class DatabaseIndexDao
{
    public const string DbIndexFileName = "index.json";

    public string DbFolder { get; set; }

    public DatabaseIndex Index { get; set; }


    public DatabaseIndexDao(string dbFolder)
    {
        DbFolder = dbFolder;

        string p = Path.Join(DbFolder, DbIndexFileName);
        if (!File.Exists(p))
        {
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            Index = new DatabaseIndex();
            _ = WriteDbIndex();
        }
        else
        {
            Index = JsonUtils.Read<DatabaseIndex>(p).Result!;
        }
    }

    public async Task WriteDbIndex()
    {
        await JsonUtils.Write(Path.Join(DbFolder, DbIndexFileName), Index, true);
    }

    public async Task<List<(string name, Database db)>> ListDatabases()
    {
        var ans = new List<(string, Database)>();

        var tasks = new List<Task>();

        foreach (var (k, v) in Index.Name2DbMap)
        {
            tasks.Add(Task.Run(() =>
            {
                var database = JsonUtils.Read<Database>(v).Result!;
                ans.Add((k, database));
            }));
        }

        await Task.WhenAll(tasks);

        return ans;
    }

    public Dictionary<string, string> GetIndex()
    {
        return Index.Name2DbMap;
    }
}