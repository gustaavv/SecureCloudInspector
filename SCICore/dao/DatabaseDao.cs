using System.Data;
using System.Data.SQLite;
using System.Reflection;
using Dapper;
using SCICore.entity;
using SCICore.util;
using DbType = System.Data.DbType;

namespace SCICore.dao;

public class DatabaseDao
{
    public string DbFilePath { get; set; }

    private string ConnectionString { get; set; }

    static DatabaseDao()
    {
        SqlMapper.AddTypeHandler(typeof(Node), new JsonTypeHandler<Node>());
        SqlMapper.AddTypeHandler(typeof(EncryptScheme), new JsonTypeHandler<EncryptScheme>());
    }

    public DatabaseDao(string dbFilePath)
    {
        DbFilePath = dbFilePath;
        ConnectionString = $"Data Source={dbFilePath};Version=3;";
        CreateTablesIfNeeded();
    }

    private IDbConnection CreateConnection()
    {
        return new SQLiteConnection(ConnectionString);
    }

    private void CreateTablesIfNeeded()
    {
        if (File.Exists(DbFilePath))
        {
            Console.WriteLine("using existing sqlite db");
            return;
        }

        var createTablesSql = ReadEmbeddedResource("SCICore.resources.schema.sql");
        using var conn = CreateConnection();
        conn.Execute(createTablesSql);

        Console.WriteLine($"sqlite db created at {DbFilePath}");
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public int Insert(Database record)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO Database (
                Name, SourceFolder, EncryptedFolder, Node, EncryptScheme, Password, DbType, CreatedAt, UpdatedAt
            ) VALUES (
                @Name, @SourceFolder, @EncryptedFolder, @Node, @EncryptScheme, @Password, @DbType, @CreatedAt, @UpdatedAt
            )";
        return conn.Execute(sql, record);
    }

    public IEnumerable<Database> SelectAll()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM Database";
        return conn.Query<Database>(sql).ToList();
    }

    public Database SelectByName(string name)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM Database WHERE Name = @Name";
        return conn.QueryFirst<Database>(sql, new { Name = name });
    }

    public int Update(Database record)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE Database SET
                SourceFolder = @SourceFolder,
                EncryptedFolder = @EncryptedFolder,
                Node = @Node,
                EncryptScheme = @EncryptScheme,
                Password = @Password,
                DbType = @DbType,
                CreatedAt = @CreatedAt,
                UpdatedAt = @UpdatedAt
            WHERE Name = @Name";
        return conn.Execute(sql, record);
    }

    public int Delete(string name)
    {
        using var conn = CreateConnection();
        const string sql = "DELETE FROM Database WHERE Name = @Name";
        return conn.Execute(sql, new { Name = name });
    }

    public bool CheckNameExists(string name)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Database WHERE Name = @Name";
        return conn.ExecuteScalar<int>(sql, new { Name = name }) > 0;
    }
}

public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = value == null ? DBNull.Value : JsonUtils.ToStr(value);
        parameter.DbType = DbType.String;
    }

    public override T Parse(object value)
    {
        if (value is string json)
            return JsonUtils.ToObj<T>(json)!;

        return default!;
    }
}