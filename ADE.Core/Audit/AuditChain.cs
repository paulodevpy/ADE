using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ADE.Core.Audit;

public class AuditChain
{
    private readonly List<AuditEvent> _events = new();

    public IReadOnlyCollection<AuditEvent> Events => _events;

    public void AddEvent(string action, string data)
    {
        string previousHash =
            _events.Count == 0
            ? string.Empty
            : _events.Last().EventHash;

        var auditEvent = new AuditEvent
        {
            EventNumber = _events.Count + 1,
            TimestampUtc = DateTime.UtcNow,
            Action = action,
            Data = data,
            PreviousHash = previousHash
        };

        auditEvent.EventHash =
            CalculateEventHash(auditEvent);

        _events.Add(auditEvent);
    }

    private static string CalculateEventHash(
        AuditEvent auditEvent)
    {
        string raw =
            $"{auditEvent.EventNumber}" +
            $"{auditEvent.TimestampUtc:o}" +
            $"{auditEvent.Action}" +
            $"{auditEvent.Data}" +
            $"{auditEvent.PreviousHash}";

        byte[] bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(raw));

        return Convert.ToHexString(bytes);
    }

    public void Save(string filePath)
    {
        using var writer =
            new StreamWriter(filePath);

        foreach (var item in _events)
        {
            writer.WriteLine(
                JsonSerializer.Serialize(item));
        }
    }
    
    public void SaveSingleEvent(
    AuditEvent auditEvent,
    string filePath)
    {
        using var writer =
            new StreamWriter(
                filePath,
                append: true);

        writer.WriteLine(
            JsonSerializer.Serialize(
                auditEvent));
    }
}