namespace SCICore.util;

public static class InputUtils
{
    public static string Read(string hint)
    {
        Console.Write(hint);
        Console.Write(" ");
        return Console.ReadLine()!.Trim();
    }
}