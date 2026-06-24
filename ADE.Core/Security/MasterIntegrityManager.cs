using System.Security.Cryptography;
using System.Text;

namespace ADE.Core.Security;

public static class MasterIntegrityManager
{
    public static void Generate(
        string collectionRoot)
    {
        string metadataFolder =
            Path.Combine(
                collectionRoot,
                "metadata");

        string manifestSha256 =
            File.ReadAllText(
                Path.Combine(
                    metadataFolder,
                    "manifest.sha256"));

        string manifestSha512 =
            File.ReadAllText(
                Path.Combine(
                    metadataFolder,
                    "manifest.sha512"));

        string combined =
            manifestSha256 +
            Environment.NewLine +
            manifestSha512;

        string sha256 =
            Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        combined)));

        string sha512 =
            Convert.ToHexString(
                SHA512.HashData(
                    Encoding.UTF8.GetBytes(
                        combined)));

        string integrityFolder =
            Path.Combine(
                collectionRoot,
                "integridade");

        File.WriteAllText(
            Path.Combine(
                integrityFolder,
                "master.sha256"),
            sha256);

        File.WriteAllText(
            Path.Combine(
                integrityFolder,
                "master.sha512"),
            sha512);
    }
}