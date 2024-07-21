using System.Security.Cryptography;
using System.Text;
using SCICore.entity;

namespace SCICore.util;

public static class HashUtils
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

    public static async Task<HashResult> ComputeFileHash(string filePath)
    {
        var sha256Task = ComputeFileHash(filePath, HashAlg.Sha256);
        var sha1Task = ComputeFileHash(filePath, HashAlg.Sha1);
        var md5Task = ComputeFileHash(filePath, HashAlg.Md5);

        var hashResults = await Task.WhenAll(sha256Task, sha1Task, md5Task);

        return new HashResult(hashResults[0], hashResults[1], hashResults[2]);
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

    public static async Task<HashResult> ComputeStringHash(string text)
    {
        var sha256Task = ComputeStringHash(text, HashAlg.Sha256);
        var sha1Task = ComputeStringHash(text, HashAlg.Sha1);
        var md5Task = ComputeStringHash(text, HashAlg.Md5);

        var hashResults = await Task.WhenAll(sha256Task, sha1Task, md5Task);

        return new HashResult(hashResults[0], hashResults[1], hashResults[2]);
    }
}