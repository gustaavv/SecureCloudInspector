namespace SCICli.entity;

public record DatabaseIndex
{
    /// <summary>
    /// key: user-defined name of a db instance. <br/>
    /// value: path to the db file.
    /// </summary>
    public Dictionary<string, string> Name2DbMap { get; set; }

    public DatabaseIndex()
    {
        Name2DbMap = new Dictionary<string, string>();
    }
}