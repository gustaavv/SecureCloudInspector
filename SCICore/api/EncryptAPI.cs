using System.Text;
using SCICore.entity;
using SCICore.util;

namespace SCICore.api;

public static class EncryptApi
{
    private const int MinPatternSize = 10;
    private const int Md5Length = 32;
    private const int Sha1Length = 40;
    private const int Sha256Length = 64;

    public static Pattern MakePattern(int length)
    {
        if (length < MinPatternSize)
        {
            throw new ArgumentException($"length >= {MinPatternSize} does not hold");
        }

        var l1 = new Random().Next(2, length / 3 + 1);
        l1 = Math.Min(l1, Md5Length);
        var l2 = new Random().Next(2, length / 3 + 1);
        l2 = Math.Min(l2, Sha1Length);
        var l3 = length - l1 - l2;
        l3 = Math.Min(l3, Sha256Length);
        var prefix = new Random().Next(2) == 0;
        return new Pattern(l1, l2, l3, prefix);
    }

    public static async Task<string> MakeArchiveName(Pattern fileNamePattern, string filename)
    {
        var (sha256, sha1, md5) = await HashUtils.ComputeStringHash(filename);
        var ans = new StringBuilder();
        ans.Append(GetSubstring(md5!, fileNamePattern.Md5, fileNamePattern.Prefix));
        ans.Append(GetSubstring(sha1!, fileNamePattern.Sha1, fileNamePattern.Prefix));
        ans.Append(GetSubstring(sha256!, fileNamePattern.Sha256, fileNamePattern.Prefix));
        return ans.ToString();
    }

    public static async Task<string> MakeArchivePwd(Database db, string? filename)
    {
        var pwd = db.Password;
        var pwdPattern = db.EncryptScheme.PwdPattern;
        if (db.EncryptScheme.PwdLevel == PasswordLevel.Db)
        {
            var (sha256, sha1, md5) = await HashUtils.ComputeStringHash(pwd);
            var ans = new StringBuilder();
            ans.Append(GetSubstring(md5!, pwdPattern.Md5, pwdPattern.Prefix));
            ans.Append(GetSubstring(sha1!, pwdPattern.Sha1, pwdPattern.Prefix));
            ans.Append(GetSubstring(sha256!, pwdPattern.Sha256, pwdPattern.Prefix));
            return ans.ToString();
        }
        else if (db.EncryptScheme.PwdLevel == PasswordLevel.File)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename), "per-file PwdLevel needs a filename");
            }

            var (pSha256, pSha1, pMd5) = await HashUtils.ComputeStringHash(pwd);
            var (fSha256, fSha1, fMd5) = await HashUtils.ComputeStringHash(filename);
            var ans = new StringBuilder();

            ans.Append(GetSubstring(pMd5!, pwdPattern.Md5 >> 1, pwdPattern.Prefix));
            ans.Append(GetSubstring(fMd5!, pwdPattern.Md5 - (pwdPattern.Md5 >> 1), !pwdPattern.Prefix));
            ans.Append(GetSubstring(pSha1!, pwdPattern.Sha1 >> 1, pwdPattern.Prefix));
            ans.Append(GetSubstring(fSha1!, pwdPattern.Sha1 - (pwdPattern.Sha1 >> 1), !pwdPattern.Prefix));
            ans.Append(GetSubstring(pSha256!, pwdPattern.Sha256 >> 1, pwdPattern.Prefix));
            ans.Append(GetSubstring(fSha256!, pwdPattern.Sha256 - (pwdPattern.Sha256 >> 1), !pwdPattern.Prefix));
            return ans.ToString();
        }
        else
        {
            throw new Exception($"unknown PasswordLevel :{db.EncryptScheme.PwdLevel}");
        }
    }

    private static string GetSubstring(string s, int len, bool prefix)
    {
        if (prefix)
        {
            return s.Substring(0, len);
        }
        else
        {
            return s.Substring(s.Length - len, len);
        }
    }
}