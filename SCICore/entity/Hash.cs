namespace SCICore.entity;

public enum HashAlg
{
    Sha256 = 0,
    Sha1 = 1,
    Md5 = 2
}

public record HashResult(string? Sha256, string? Sha1, string? Md5)
{
    public HashResult() : this(null, null, null)
    {
    }
}