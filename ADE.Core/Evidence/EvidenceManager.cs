using ADE.Core.Models;
using ADE.Core.Security;

namespace ADE.Core.Evidence;

public class EvidenceManager
{
    public EvidenceFile ImportFile(
    string sourceFile,
    string evidenceFolder)
    {
        string originalName =
            Path.GetFileName(sourceFile);

        string destinationFile =
            Path.Combine(
                evidenceFolder,
                originalName);

        int counter = 1;

        while (File.Exists(destinationFile))
        {
            string name =
                Path.GetFileNameWithoutExtension(
                    originalName);

            string ext =
                Path.GetExtension(
                    originalName);

            destinationFile =
                Path.Combine(
                    evidenceFolder,
                    $"{name}_{counter:000}{ext}");

            counter++;
        }

        File.Copy(
            sourceFile,
            destinationFile);

        var info =
            new FileInfo(destinationFile);

        return new EvidenceFile
        {
            FileName = info.Name,

            OriginalPath =
                sourceFile,

            ImportedPath =
                destinationFile,

            RelativePath =
                Path.Combine(
                    "evidencias",
                    info.Name),

            SizeBytes =
                info.Length,

            EvidenceType =
                info.Extension,

            Sha256 =
                IntegrityManager
                    .CalculateSha256(
                        destinationFile),

            Sha512 =
                IntegrityManager
                    .CalculateSha512(
                        destinationFile),

            CollectedAtUtc =
                DateTime.UtcNow,

            CollectionMethod =
                 "IMPORTAÇÃO"
        };

    }
}