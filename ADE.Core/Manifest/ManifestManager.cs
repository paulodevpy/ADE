using ADE.Core.Security;

namespace ADE.Core.Manifest;

public class ManifestManager
{
    public string SaveManifest(
        ManifestModel manifest,
        string metadataFolder)
    {
        Directory.CreateDirectory(
            metadataFolder);

        string manifestPath =
            Path.Combine(
                metadataFolder,
                "manifest.json");

        ManifestWriter.Save(
            manifest,
            manifestPath);

        string sha256 =
            IntegrityManager
                .CalculateSha256(
                    manifestPath);

        string sha512 =
            IntegrityManager
                .CalculateSha512(
                    manifestPath);

        File.WriteAllText(
            Path.Combine(
                metadataFolder,
                "manifest.sha256"),
            sha256);

        File.WriteAllText(
            Path.Combine(
                metadataFolder,
                "manifest.sha512"),
            sha512);

        return manifestPath;
    }
}