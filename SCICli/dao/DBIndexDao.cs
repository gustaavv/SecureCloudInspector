using SCICli.entity;
using SCICore.util;

namespace SCICli.dao;

public class DatabaseIndexDao
{
    public const string DbIndexFileName = "index.json";

    private string DbFolder { get; set; }

    private DatabaseIndex Index { get; set; }


    public DatabaseIndexDao(string dbFolder)
    {
        DbFolder = dbFolder;

        string p = Path.Join(DbFolder, DbIndexFileName);
        if (!File.Exists(p))
        {
            Index = new DatabaseIndex();
            _ = WriteDbIndex();
        }
        else
        {
            Index = JsonUtils.Read<DatabaseIndex>(p).Result!;
        }
    }

    private async Task WriteDbIndex()
    {
        await JsonUtils.Write(Path.Join(DbFolder, DbIndexFileName), Index, true);
    }

    public List<(string, string)> ListDatabases()
    {
        var ans = new List<(string, string)>();

        foreach (var (k, v) in Index.Name2DbMap)
        {
            ans.Add((k, v));
        }

        return ans;
    }
}