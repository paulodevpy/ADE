using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ADE.Core.Audit;

public static class AuditChainVerifier
{
    public static AuditVerificationResult Verify(
        string auditFile)
    {
        var result =
            new AuditVerificationResult();

        if (!File.Exists(auditFile))
        {
            result.Messages.Add(
                "audit.jsonl não encontrado.");

            return result;
        }

        string previousHash = "";
        int expectedNumber = 1;

        foreach (string line in File.ReadLines(auditFile))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            AuditEvent? evt =
                JsonSerializer.Deserialize<AuditEvent>(
                    line);

            if (evt is null)
                continue;

            if (evt.EventNumber != expectedNumber)
            {
                result.Messages.Add(
                    $"Sequência inválida no evento {evt.EventNumber}");
            }

            if (!string.Equals(
                    evt.PreviousHash,
                    previousHash,
                    StringComparison.Ordinal))
            {
                result.Messages.Add(
                    $"PreviousHash inválido no evento {evt.EventNumber}");
            }

            string raw =
                $"{evt.EventNumber}" +
                $"{evt.TimestampUtc:o}" +
                $"{evt.Action}" +
                $"{evt.Data}" +
                $"{evt.PreviousHash}";

            string calculatedHash =
                Convert.ToHexString(
                    SHA256.HashData(
                        Encoding.UTF8.GetBytes(raw)));

            if (!string.Equals(
                    calculatedHash,
                    evt.EventHash,
                    StringComparison.OrdinalIgnoreCase))
            {
                result.Messages.Add(
                    $"Hash inválido no evento {evt.EventNumber}");
            }

            previousHash =
                evt.EventHash;

            expectedNumber++;
        }

        result.Success =
            result.Messages.Count == 0;

        if (result.Success)
        {
            result.Messages.Add(
                "Cadeia de auditoria íntegra.");
        }

        return result;
    }
}