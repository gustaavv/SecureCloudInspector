using SCICore.entity;
using SCICore.util;

namespace SCICli.dao;

public class DbDao
{
    private string DbFilePath { get; set; }

    private Database Db { get; set; }


    public DbDao(string dbFilePath)
    {
        DbFilePath = dbFilePath;
        if (!File.Exists(dbFilePath))
        {
            throw new ArgumentException("not a valid file", nameof(dbFilePath));
        }

        Db = JsonUtils.Read<Database>(dbFilePath).Result!;
    }

    public async Task WriteDb()
    {
        await JsonUtils.Write(DbFilePath, Db, true);
    }
}