using System.Security.Cryptography;
using System.Text;

namespace SCICore.util;

public enum HashAlg
{
    Sha256,
    Sha1,
    Md5
}

public static class EncryptUtil
{
    private static HashAlgorithm GetHashAlgorithmInstance(HashAlg hashAlg) =>
        hashAlg switch
        {
            HashAlg.Sha256 => SHA256.Create(),
            HashAlg.Sha1 => SHA1.Create(),
            HashAlg.Md5 => MD5.Create(),
            _ => throw new ArgumentException($"Unsupported hash algorithm: {hashAlg}", nameof(hashAlg))
        };

    public static async Task<string> ComputeFileHash(string filePath, HashAlg hashAlg)
    {
        return await Task.Run(() =>
        {
            using var ha = GetHashAlgorithmInstance(hashAlg);
            using var fileStream = File.OpenRead(filePath);
            var hashBytes = ha.ComputeHash(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        });
    }

    public static async Task<string> ComputeStringHash(string text, HashAlg hashAlg)
    {
        return await Task.Run(() =>
        {
            using var ha = GetHashAlgorithmInstance(hashAlg);
            var inputBytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = ha.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        });
    }
}