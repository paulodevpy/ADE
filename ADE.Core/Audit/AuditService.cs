using System.Text.Json;

namespace ADE.Core.Audit;

public static class AuditService
{
    public static void AppendEvent(
        string auditFile,
        string action,
        string data)
    {
        AuditEvent? previous = null;

        if (File.Exists(auditFile))
        {
            string? lastLine =
                File.ReadLines(auditFile)
                    .LastOrDefault();

            if (!string.IsNullOrWhiteSpace(lastLine))
            {
                previous =
                    JsonSerializer.Deserialize<AuditEvent>(
                        lastLine);
            }
        }

        var audit =
            new AuditChain();

        string previousHash =
            previous?.EventHash ?? "";

        var evt =
            new AuditEvent
            {
                EventNumber =
                    previous?.EventNumber + 1 ?? 1,

                TimestampUtc =
                    DateTime.UtcNow,

                Action =
                    action,

                Data =
                    data,

                PreviousHash =
                    previousHash
            };

        var raw =
            $"{evt.EventNumber}" +
            $"{evt.TimestampUtc:o}" +
            $"{evt.Action}" +
            $"{evt.Data}" +
            $"{evt.PreviousHash}";

        evt.EventHash =
            Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(raw)));

        audit.SaveSingleEvent(
            evt,
            auditFile);
    }
}