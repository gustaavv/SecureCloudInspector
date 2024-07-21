namespace SCICore.entity;

public enum ItemType
{
    File,
    Dir
}

public record Item(string Name, ItemType Type, List<Item> Children)
{
    public Item(string Name, ItemType Type)
        : this(Name, Type, new List<Item>())
    {
    }
}