using System.Security.Cryptography;

namespace ADE.Core.Security;

public static class IntegrityManager
{
    public static string CalculateSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        return Convert.ToHexString(
            SHA256.HashData(stream));
    }

    public static string CalculateSha512(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        return Convert.ToHexString(
            SHA512.HashData(stream));
    }
}