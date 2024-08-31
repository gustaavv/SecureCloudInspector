namespace SCICore.entity;

public record DatabaseRecord
{
    public string Filepath { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DatabaseRecord()
    {
    }
}

public record DatabaseIndex
{
    /// <summary>
    /// key: user-defined name of a db instance. <br/>
    /// value: path to the db file.
    /// </summary>
    public Dictionary<string, DatabaseRecord> Map { get; set; }

    public DatabaseIndex()
    {
        Map = new Dictionary<string, DatabaseRecord>();
    }
}