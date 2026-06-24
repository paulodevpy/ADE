namespace ADE.Core.Audit;

public class AuditEvent
{
    public int EventNumber { get; set; }

    public DateTime TimestampUtc { get; set; }

    public DateTime? TrustedTimeUtc { get; set; }

    public string TimeSource { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;

    public string PreviousHash { get; set; } = string.Empty;

    public string EventHash { get; set; } = string.Empty;
}