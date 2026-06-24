using System.IO.Compression;
using ADE.Core.Security;

namespace ADE.Core.Cases;

public static class CaseExporter
{
    public static string Export(
        string caseFolder,
        string zipFile)
    {
        if (File.Exists(zipFile))
            File.Delete(zipFile);

        ZipFile.CreateFromDirectory(
            caseFolder,
            zipFile);

        string sha256 =
            IntegrityManager
                .CalculateSha256(zipFile);

        string sha512 =
            IntegrityManager
                .CalculateSha512(zipFile);

        File.WriteAllText(
            zipFile + ".sha256",
            sha256);

        File.WriteAllText(
            zipFile + ".sha512",
            sha512);

        return zipFile;
    }
}