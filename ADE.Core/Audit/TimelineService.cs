using System.Text.Json;

namespace ADE.Core.Audit;

public static class TimelineService
{
    public static List<TimelineEntry> Build(
        string auditFile)
    {
        var result =
            new List<TimelineEntry>();

        if (!File.Exists(auditFile))
            return result;

        foreach (var line in File.ReadLines(auditFile))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var audit =
                JsonSerializer.Deserialize<AuditEvent>(
                    line);

            if (audit is null)
                continue;

            result.Add(
                new TimelineEntry
                {
                    TimestampUtc =
                        audit.TimestampUtc,

                    EventType =
                        audit.Action,

                    Description =
                        audit.Data
                });
        }

        return result
            .OrderBy(x => x.TimestampUtc)
            .ToList();
    }
}