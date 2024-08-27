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

    public async Task<List<(string name, DatabaseRecord record, Database db)>> ListDatabases()
    {
        var ans = new List<(string, DatabaseRecord, Database)>();

        var tasks = new List<Task>();

        foreach (var (k, v) in Index.Map)
        {
            tasks.Add(Task.Run(() =>
            {
                var database = JsonUtils.Read<Database>(v.Filepath).Result!;
                ans.Add((k, v, database));
            }));
        }

        await Task.WhenAll(tasks);

        return ans;
    }

    public Dictionary<string, DatabaseRecord> GetIndex()
    {
        return Index.Map;
    }
}