using SCICli.entity;
using SCICore.util;

namespace SCICli.dao;

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

    public List<(string name, string path)> ListDatabases()
    {
        var ans = new List<(string, string)>();

        foreach (var (k, v) in Index.Name2DbMap)
        {
            ans.Add((k, v));
        }

        return ans;
    }

    public Dictionary<string, string> GetIndex()
    {
        return Index.Name2DbMap;
    }
}