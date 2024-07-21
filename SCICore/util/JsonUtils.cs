using System.Text.Json;
using System.Text.Json.Serialization;

namespace SCICore.util;

public static class JsonUtils
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static string ToStr(Object obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static T? ToObj<T>(string s)
    {
        return JsonSerializer.Deserialize<T>(s, Options);
    }


    public static async Task<T?> Read<T>(string file)
    {
        await using var stream = File.OpenRead(file);
        return await JsonSerializer.DeserializeAsync<T>(stream, options: Options);
    }

    public static async Task Write(string file, Object obj)
    {
        await using var stream = File.Create(file);
        await JsonSerializer.SerializeAsync(stream, obj, Options);
    }
}