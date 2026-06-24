using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using ADE.Core.Manifest;

namespace ADE.Core.Security;

public static class IntegrityVerifier
{
    public static VerificationResult Verify(
        string caseFolder)
    {
        var result =
            new VerificationResult();

        try
        {
            string metadataFolder =
                Path.Combine(
                    caseFolder,
                    "metadata");

            string manifestFile =
                Path.Combine(
                    metadataFolder,
                    "manifest.json");

            string manifestSha256File =
                Path.Combine(
                    metadataFolder,
                    "manifest.sha256");

            string manifestSha512File =
                Path.Combine(
                    metadataFolder,
                    "manifest.sha512");

            if (!File.Exists(manifestFile))
            {
                result.Messages.Add(
                    "Manifesto não encontrado.");

                return result;
            }

            if (!File.Exists(manifestSha256File))
            {
                result.Messages.Add(
                    "manifest.sha256 ausente");
            }

            if (!File.Exists(manifestSha512File))
            {
                result.Messages.Add(
                    "manifest.sha512 ausente");
            }

            string currentManifestSha256 =
                IntegrityManager
                    .CalculateSha256(
                        manifestFile);

            string currentManifestSha512 =
                IntegrityManager
                    .CalculateSha512(
                        manifestFile);

            if (File.Exists(manifestSha256File))
            {
                string storedManifestSha256 =
                    File.ReadAllText(
                        manifestSha256File)
                        .Trim();

                if (!currentManifestSha256.Equals(
                        storedManifestSha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result.Messages.Add(
                        "manifest.sha256 divergente");
                }
            }

            if (File.Exists(manifestSha512File))
            {
                string storedManifestSha512 =
                    File.ReadAllText(
                        manifestSha512File)
                        .Trim();

                if (!currentManifestSha512.Equals(
                        storedManifestSha512,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result.Messages.Add(
                        "manifest.sha512 divergente");
                }
            }

            string integrityFolder =
                Path.Combine(
                    caseFolder,
                    "integridade");

            string masterSha256File =
                Path.Combine(
                    integrityFolder,
                    "master.sha256");

            string masterSha512File =
                Path.Combine(
                    integrityFolder,
                    "master.sha512");

            string combined =
                currentManifestSha256 +
                Environment.NewLine +
                currentManifestSha512;

            string expectedMasterSha256 =
                Convert.ToHexString(
                    SHA256.HashData(
                        Encoding.UTF8.GetBytes(
                            combined)));

            string expectedMasterSha512 =
                Convert.ToHexString(
                    SHA512.HashData(
                        Encoding.UTF8.GetBytes(
                            combined)));

            if (File.Exists(masterSha256File))
            {
                string storedMasterSha256 =
                    File.ReadAllText(
                        masterSha256File)
                        .Trim();

                if (!expectedMasterSha256.Equals(
                        storedMasterSha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result.Messages.Add(
                        "master.sha256 divergente");
                }
            }
            else
            {
                result.Messages.Add(
                    "master.sha256 ausente");
            }

            if (File.Exists(masterSha512File))
            {
                string storedMasterSha512 =
                    File.ReadAllText(
                        masterSha512File)
                        .Trim();

                if (!expectedMasterSha512.Equals(
                        storedMasterSha512,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result.Messages.Add(
                        "master.sha512 divergente");
                }
            }
            else
            {
                result.Messages.Add(
                    "master.sha512 ausente");
            }

            string json =
                File.ReadAllText(
                    manifestFile);

            var manifest =
                JsonSerializer.Deserialize<
                    ManifestModel>(json);

            if (manifest is null)
            {
                result.Messages.Add(
                    "Manifesto inválido.");

                return result;
            }

            foreach (var evidence
                in manifest.EvidenceFiles)
            {
                string evidencePath =
                    Path.Combine(
                        caseFolder,
                        "evidencias",
                        evidence.FileName);

                if (!File.Exists(
                    evidencePath))
                {
                    result.Messages.Add(
                        $"Arquivo ausente: {evidence.FileName}");

                    continue;
                }

                string currentHash =
                    IntegrityManager
                        .CalculateSha256(
                            evidencePath);

                if (!currentHash.Equals(
                    evidence.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                {
                    result.Messages.Add(
                        $"Hash divergente: {evidence.FileName}");
                }
            }

            result.Success =
                result.Messages.Count == 0;

            if (result.Success)
            {
                result.Messages.Add(
                    "Integridade validada.");
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Messages.Add(
                ex.Message);

            return result;
        }
    }
}