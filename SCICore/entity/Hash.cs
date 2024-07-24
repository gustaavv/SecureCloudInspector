namespace SCICore.entity;

public enum HashAlg
{
    Sha256,
    Sha1,
    Md5
}

public record HashResult(string? Sha256, string? Sha1, string? Md5)
{
    public HashResult() : this(null, null, null)
    {
    }
    
    
}